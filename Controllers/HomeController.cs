using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using tx_web_sign.Models;
using TXTextControl;
using TXTextControl.DocumentServer;

namespace tx_web_sign.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public string SignDocument(Document model)
        {
            byte[] document = Convert.FromBase64String(model.BinaryDocument);
            byte[] signatureDocument = null;

            using (ServerTextControl tx = new ServerTextControl())
            {
                tx.Create();

                // use MailMerge to merge the signature template
                using (MailMerge mailMerge = new MailMerge())
                {
                    mailMerge.TextComponent = tx;
                    mailMerge.SearchPath = Server.MapPath("/Signature_Images/");
                    mailMerge.LoadTemplate(Server.MapPath("/App_Data/documents/signature_template.tx"), FileFormat.InternalUnicodeFormat);

                    // create a new signature object
                    Signature signature = new Signature();
                    signature.Name = "Peter Chadwick";
                    signature.SignatureImage = signature.Name.ToLower().Replace(" ", "_") + ".png";

                    // merge and save the resulting document
                    mailMerge.MergeObject(signature);
                    mailMerge.SaveDocumentToMemory(out signatureDocument, BinaryStreamType.InternalUnicodeFormat, null);
                }

                // load the original document from the editor
                tx.Load(document, BinaryStreamType.InternalUnicodeFormat);

                // find the "signature" text frame with the name "txsig"
                foreach (TextFrame frame in tx.TextFrames)
                {
                    if (frame.Name == "txsig")
                    {
                        frame.Tables.Clear();
                        frame.Selection.Start = 0;
                        frame.Selection.Length = -1;
                        
                        // load the merged signature template into the
                        // text frame and save the complete document
                        frame.Selection.Load(signatureDocument, BinaryStreamType.InternalUnicodeFormat);
                        tx.Save(out document, BinaryStreamType.InternalUnicodeFormat);
                        break;
                    }
                }
                
            }

            // finally, return the signed document
            return Convert.ToBase64String(document);
        }

        public PartialViewResult SignatureDialogPartialView()
        {
            Signature signature = new Signature();
            signature.Name = "Peter Chadwick";
            signature.SignatureImage = signature.Name.ToLower().Replace(" ", "_") + ".png";

            return PartialView("SignatureDialog", signature);
        }
    }
}