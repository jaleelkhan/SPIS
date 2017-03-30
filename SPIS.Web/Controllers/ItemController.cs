using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SPIS.BL;
using SPIS.BL.Models;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage;
using System.Configuration;
using Microsoft.WindowsAzure.Storage.Queue;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace SPIS.Web.Controllers
{
    public class ItemController : Controller
    {
        private SPISEntities db = new SPISEntities();
        private CloudQueue thumbnailRequestQueue;
        private static CloudBlobContainer imagesBlobContainer;
        // GET: Item

        public ItemController()
        {
            InitializeStorage();
        }

        private void InitializeStorage()
        {
            // Open storage account using credentials from .cscfg file.
            var storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ToString());

            // Get context object for working with blobs, and 
            // set a default retry policy appropriate for a web user interface.
            var blobClient = storageAccount.CreateCloudBlobClient();
            //blobClient.DefaultRequestOptions.RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(3), 3);

            // Get a reference to the blob container.
            imagesBlobContainer = blobClient.GetContainerReference("item-images");

            // Get context object for working with queues, and 
            // set a default retry policy appropriate for a web user interface.
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            //queueClient.DefaultRequestOptions.RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(3), 3);

            // Get a reference to the queue.
            thumbnailRequestQueue = queueClient.GetQueueReference("thumbnailrequest");
        }

        public ActionResult Index()
        {
            try
            {
                List<Good> lstGoods = new List<Good>();
                // TODO: Add insert logic here
                using (SPISEntities objContext = new SPISEntities())
                {
                    objContext.Database.Connection.Open();
                    lstGoods = objContext.Goods.ToList();
                    
                }
                return View(lstGoods);
            }
            catch (Exception ex)
            {               
                return View();
            }
        }

        // GET: Item/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Item/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Item/Create
        [HttpPost]
        public async Task<ActionResult> Create(FormCollection collection, IEnumerable<HttpPostedFileBase> files)
        {
            try
            {
                // TODO: Add insert logic here
                using (SPISEntities objContext = new SPISEntities())
                {
                    CloudBlockBlob imageBlob = null;
                    objContext.Database.Connection.Open();
                    Good Tbl = new Good();
                    //  
                    Tbl.Category = collection["Category"].ToString();
                    Tbl.Item = collection["Item"].ToString();
                    Tbl.Quantity = int.Parse(collection["Quantity"]);
                    Tbl.CreatedOn = DateTime.Now;
                    foreach(var imageFile in files)
                    {
                        if (imageFile != null && imageFile.ContentLength != 0)
                        {
                            imageBlob = await UploadAndSaveBlobAsync(imageFile);
                            Tbl.ImageUrl = imageBlob.Uri.ToString();
                        }
                    }
                    

                    //  
                    objContext.Goods.Add(Tbl);
                    int i = objContext.SaveChanges();

                    if (imageBlob != null)
                    {
                        BlobInformation blobInfo = new BlobInformation() { AdId = Tbl.Id, BlobUri = new Uri(Tbl.ImageUrl) };
                        var queueMessage = new CloudQueueMessage(JsonConvert.SerializeObject(blobInfo));
                        await thumbnailRequestQueue.AddMessageAsync(queueMessage);
                        Trace.TraceInformation("Created queue message for AdId {0}", Tbl.Id);
                    }
                    //  
                   
                    if (i > 0)
                    {
                        ViewBag.Msg = "Data Saved Suuceessfully.";
                    }
                }
                return RedirectToAction("Index");
            }
            catch(Exception ex)
            {
                Trace.TraceError(ex.Message);
                return View();
            }
        }

        // GET: Item/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Item/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Item/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Item/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        private async Task<CloudBlockBlob> UploadAndSaveBlobAsync(HttpPostedFileBase imageFile)
        {
            Trace.TraceInformation("Uploading image file {0}", imageFile.FileName);

            string blobName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            // Retrieve reference to a blob. 
            CloudBlockBlob imageBlob = imagesBlobContainer.GetBlockBlobReference(blobName);
            // Create the blob by uploading a local file.
            using (var fileStream = imageFile.InputStream)
            {
                await imageBlob.UploadFromStreamAsync(fileStream);
            }

            Trace.TraceInformation("Uploaded image file to {0}", imageBlob.Uri.ToString());

            return imageBlob;
        }
    }
}
