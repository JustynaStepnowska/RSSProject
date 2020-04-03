using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Mvc;
using RSSProject.Models;
using RSSProject.Services;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace RSSProject.Controllers
{
    public class RSSController : Controller
    {
        private readonly ICosmosDbService _cosmosDbService;
        public RSSController(ICosmosDbService cosmosDbService)
        {
            _cosmosDbService = cosmosDbService;
        }

        [ActionName("Create")]
        public async Task<ActionResult> Create(string id = "")
        {
            string text = "";
            string Url = "";
            IEnumerable<RSS> ListOfRSS = new List<RSS>();
            ListOfRSS = await _cosmosDbService.GetItemsAsync("SELECT * FROM c");
            ViewBag.ListOfRSS = ListOfRSS;
            if (!String.IsNullOrEmpty(id))
            {
                string query = "SELECT * FROM c WHERE c.id='" + id + "'";
                ListOfRSS = await _cosmosDbService.GetItemsAsync(query);
                RSS Current = ListOfRSS.Last();
                Url = Current.RSSAddress;
                XmlReader reader = XmlReader.Create(Url);
                SyndicationFeed feed = SyndicationFeed.Load(reader);
                reader.Close();
                foreach (SyndicationItem item in feed.Items)
                {
                    text += item.Title.Text;
                    text += Environment.NewLine;
                    foreach (var x in item.Links)
                    {
                        text += x.Uri;
                    }
                }
                
            }
            ViewBag.Url = Url;
            ViewBag.EmailText = text;
            return View();
        }

        [HttpPost]
        [ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateAsync([Bind("Id,RSSAddress,Email")] RSS item,string create, string send, string EmailText)
        {
            if (ModelState.IsValid)
            {
                if (!String.IsNullOrEmpty(create))
                {
                    item.Id = Guid.NewGuid().ToString();
                    await _cosmosDbService.AddItemAsync(item);
                    return RedirectToAction("Create", new { id = item.Id });
                }
                else if(!String.IsNullOrEmpty(send))
                {
                    var client = new SendGridClient("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");
                    var msg = new SendGridMessage();
                    msg.SetFrom(new EmailAddress("content@rss.com", "RSSContent"));
                    msg.AddTo(item.Email);
                    msg.SetSubject("Content from given RSS");
                    msg.AddContent(MimeType.Text, EmailText);
                    var response = await client.SendEmailAsync(msg);
                    return RedirectToAction("Create", new { id = "" }); ;
                }
                
            }

            return View(item);
        }

    }
}