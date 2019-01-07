using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;

namespace AWSS3Bucket.Controllers
{
    public class BucketModel
    {
        public BucketModel() { }
        public BucketModel(string bucketName, DateTime createdate, string regionName, string url)
        {
            this.BucketName = bucketName;
            this.CreationDate = createdate;
            this.Url = url;
            this.RegionName = regionName;
        }
        public DateTime CreationDate { get; set; }
        public string RegionName { get; set; }
        public string BucketName { get; set; }
        public string Url { get; set; }

    }
    public class S3BucketController : Controller
    {
        private IAmazonS3 _awsService { get; set; }
        
        public S3BucketController()
        {
            //Amazon.AWSClientFactory.CreateAmazonS3Client(Config.AccessKey, Config.SecretKey, RegionEndpoint.APNortheast1);
            _awsService = Amazon.AWSClientFactory.CreateAmazonS3Client( ConfigurationManager.AppSettings["awsAccessKey"],
                ConfigurationManager.AppSettings["awsSecretKey"],
                Amazon.RegionEndpoint.APSoutheast1);
            
        }

        // GET: S3Bucket
        public ActionResult Index()
        {
            var model = new List<BucketModel>();
            ListBucketsResponse bucketsResponse = _awsService.ListBuckets();
            foreach (var bucket in bucketsResponse.Buckets)
            {
                var region = this._awsService.GetBucketLocation(bucket.BucketName).Location.Value;
                GetPreSignedUrlRequest req = new GetPreSignedUrlRequest();
                req.BucketName = bucket.BucketName;
                req.Expires = DateTime.Now.AddDays(5);
                req.Protocol = Protocol.HTTPS;
                model.Add(new BucketModel(bucket.BucketName, bucket.CreationDate, region, _awsService.GetPreSignedURL(req)));
            }
            return View(model);
        }

        // GET: S3Bucket/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: S3Bucket/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: S3Bucket/Create
        [HttpPost]
        public ActionResult Create(BucketModel collection)
        {
            try
            {

                PutBucketRequest bucketRequest = new PutBucketRequest();
                bucketRequest.BucketName = collection.BucketName;

                _awsService.PutBucket(bucketRequest);

                return RedirectToAction("Index");
            }
            catch(Exception ex)
            {
                return View();
            }
        }

        // GET: S3Bucket/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: S3Bucket/Edit/5
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

        
        
        public ActionResult Delete(string bucketName)
        {
            try
            {
                var location = this._awsService.GetBucketLocation(bucketName);
                DeleteBucketRequest deleteBucketRequest = new DeleteBucketRequest();
                deleteBucketRequest.BucketName = bucketName;
                deleteBucketRequest.BucketRegion = new S3Region(location.Location);
                this._awsService.DeleteBucket(deleteBucketRequest);

            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Index");
        }
    }
}
