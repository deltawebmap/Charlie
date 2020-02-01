using DeltaWebMap.Charlie.Framework.Firebase.FirebaseCreateVersionRequest;
using DeltaWebMap.Charlie.Framework.Firebase.FirebaseCreateVersionResponse;
using DeltaWebMap.Charlie.Framework.Firebase.FirebaseFinalizeRequest;
using DeltaWebMap.Charlie.Framework.Firebase.FirebasePopulateFilesRequest;
using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DeltaWebMap.Charlie.Framework.Managers.AssetManagerTransports
{
    public class AssetManagerTransportFirebase : AssetManagerTransport
    {
        public CharlieConfig config;
        public string fb_token;
        public string version;
        public List<Tuple<string, string, Stream>> queue;
        public long totalSize;
        public LockObject logLock;

        public AssetManagerTransportFirebase()
        {
            this.logLock = new LockObject();
        }

        public override string AddFile(string pathname, Stream data)
        {
            //GZIP this
            MemoryStream compressed = new MemoryStream();
            using (GZipStream gz = new GZipStream(compressed, CompressionLevel.Optimal, true))
            {
                data.CopyTo(gz);
            }
            compressed.Position = 0;

            //Close old
            data.Close();
            data.Dispose();

            //Compress
            totalSize += compressed.Length;

            //Add to queue
            queue.Add(new Tuple<string, string, Stream>(pathname, GetFileHashString(compressed), compressed));

            //Return paths
            return "https://" + config.assets_url_host + pathname;
        }

        public override void EndSession()
        {
            //If we have no files, do nothing
            if (queue.Count == 0)
                return;

            //Get token
            Log($"About to send {queue.Count} files, obtaining access token...");
            fb_token = GetAuthToken(config.firebase_cfg);

            //Create a new version
            Log($"About to send {queue.Count} files, obtaining new version...");
            version = CreateVersion().name;

            //Log
            Log($"About to populate {queue.Count} files, {totalSize / 1000 } KB total");
            
            //We'll commit all files
            FirebasePopulateFilesResponse population = PopulateFiles();
            if(population.uploadRequiredHashes != null)
            {
                //Send all files
                long remaining = totalSize;
                int remainingItems = queue.Count;
                Parallel.For(0, queue.Count, (int i) =>
                {
                    var v = queue[i];
                    bool shouldUpload;
                    lock (queue)
                    {
                        string hash = v.Item2.ToLower();
                        shouldUpload = population.uploadRequiredHashes.Contains(hash);
                        if (shouldUpload)
                            population.uploadRequiredHashes.Remove(hash);
                    }
                    if (shouldUpload)
                    {
                        Log($"About to upload #{i}, {v.Item1} with length of {v.Item3.Length / 1000} KB. {remaining / 1000} / {totalSize / 1000} KB ({remainingItems} items) remain");
                        SendHTTPRequestMetal(population.uploadUrl + "/" + v.Item2.ToLower(), "POST", "application/octet-stream", v.Item3);
                    }
                    else
                    {
                        Log($"Skipping #{i} because it is already used");
                    }
                    remaining -= v.Item3.Length;
                    remainingItems--;
                });
            } else
            {
                Log("No files to upload!");
            }          

            //Finalize
            Log($"About to finalize");
            SendHTTPRequest("https://firebasehosting.googleapis.com/v1beta1/" + version, "PATCH", new FirebaseFinalizeRequest
            {
                status = "FINALIZED"
            });

            //Push to prod
            Log($"About to push to prod");
            SendHTTPRequestMetal("https://firebasehosting.googleapis.com/v1beta1/sites/" + config.firebase_project_id + "/releases?versionName="+version, "POST", "application/octet-stream", new MemoryStream());
        }

        public override void StartSession(CharlieConfig cfg)
        {
            //Set config
            this.config = cfg;

            //Get ready
            this.queue = new List<Tuple<string, string, Stream>>();
        }

        private void Log(string msg)
        {
            lock(logLock)
                Console.WriteLine($"[AssetManagerTransportFirebase] " + msg);
        }

        private FirebasePopulateFilesResponse PopulateFiles()
        {
            //Create request
            FirebasePopulateFilesRequest request = new FirebasePopulateFilesRequest
            {
                files = new Dictionary<string, string>()
            };
            foreach (var e in queue)
                request.files.Add(e.Item1, e.Item2);

            //Send
            return SendHTTPRequest<FirebasePopulateFilesResponse, FirebasePopulateFilesRequest>("https://firebasehosting.googleapis.com/v1beta1/"+version+":populateFiles ", "POST", request);
        }

        private string GetFileHashString(Stream s)
        {
            var sha = new SHA256Managed();
            byte[] checksum = sha.ComputeHash(s);
            char[] output = new char[checksum.Length * 2];
            for(int i = 0; i<checksum.Length; i++)
            {
                string sb = checksum[i].ToString("X2");
                output[(i * 2) + 0] = sb[0];
                output[(i * 2) + 1] = sb[1];
            }
            return new string(output);
        }

        private FirebaseCreateVersionResponse CreateVersion()
        {
            //Create request
            FirebaseCreateVersionRequest request = new FirebaseCreateVersionRequest
            {
                config = new Firebase.FirebaseCreateVersionRequest.Config
                {
                    headers = new List<Firebase.FirebaseCreateVersionRequest.Header>
                    {
                        new Firebase.FirebaseCreateVersionRequest.Header
                        {
                            glob = "**",
                            headers = new Dictionary<string, string>
                            {
                                {"Cache-Control", "max-age=1800" }
                            }
                        }
                    }
                }
            };

            //Send
            return SendHTTPRequest<FirebaseCreateVersionResponse, FirebaseCreateVersionRequest>("https://firebasehosting.googleapis.com/v1beta1/sites/"+ config.firebase_project_id + "/versions", "POST", request);
        }

        private T SendHTTPRequest<T, O>(string url, string method, O data)
        {
            //Get
            var response = SendHTTPRequest<O>(url, method, data);

            //Decode
            return JsonConvert.DeserializeObject<T>(response);
        }

        private string SendHTTPRequest<O>(string url, string method, O data)
        {
            //Create a request stream with the payload
            string response;
            using(MemoryStream ms = new MemoryStream())
            {
                //Write
                byte[] content = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));
                ms.Write(content, 0, content.Length);

                //Rewind
                ms.Position = 0;

                //Send
                response = SendHTTPRequestMetal(url, method, "application/json", ms);
            }

            //Decode
            return response;
        }

        private string SendHTTPRequestMetal(string url, string method, string contentType, Stream content)
        {
            var client = new HttpClient();
            HttpRequestMessage msg = new HttpRequestMessage(new HttpMethod(method), url);
            content.Position = 0;
            msg.Content = new StreamContent(content);
            msg.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
            msg.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", fb_token);
            var response = client.SendAsync(msg).GetAwaiter().GetResult();
            string sr = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            if (!response.IsSuccessStatusCode)
                throw new Exception("HTTP request to Google Firebase was not successful!");
            return sr;
        }

        private string GetAuthToken(string firebaseCfgPath)
        {
            using (var stream = new FileStream(firebaseCfgPath, FileMode.Open, FileAccess.Read))
            {
                return GoogleCredential
                    .FromStream(stream) // Loads key file  
                    .CreateScoped(new string[] {
                        "https://www.googleapis.com/auth/firebase"
                        }) // Gathers scopes requested  
                    .UnderlyingCredential // Gets the credentials  
                    .GetAccessTokenForRequestAsync().GetAwaiter().GetResult(); // Gets the Access Token  
            }
        }
    }
}
