using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApiAzure.Models;

namespace WebApiAzure.Controllers
{
    public class BooksController : ApiController
    {
        // GET: api/Books
        public IEnumerable<BookInfo> Get()
        {
            List<BookInfo> books = DB.Books.GetBooks(DTC.BookNature.TypeFree, DTC.StatusEnum.Running).Values.ToList();

            return books;
        }

        [HttpGet]
        [Route("api/Books/{bookNatureID}/{statusID}")]
        public IEnumerable<BookInfo> Get(int bookNatureID, int statusID)
        {
            List<BookInfo> books = new List<BookInfo>();
            DTC.BookNature bookNature = (DTC.BookNature)bookNatureID;
            DTC.StatusEnum status = (DTC.StatusEnum)statusID;

            books = DB.Books.GetBooks(bookNature, status).Values.ToList();

            return books;
        }

        [HttpGet]
        [Route("api/Books/{bookID}")]
        public BookInfo Get(int bookID)
        {
            return DB.Books.GetBook(bookID);
        }

        [HttpPost]
        [Route("api/Books/")]
        public void Post([FromBody]BookInfo book)
        {
            DB.Books.AddUpdateBook(book);
        }

        [HttpPut]
        [Route("api/Books/{bookID}")]
        public void Put(int bookID, [FromBody]BookInfo book)
        {
            DB.Books.AddUpdateBook(book);
        }

        [HttpDelete]
        [Route("api/Books/{bookID}")]
        public void Delete(int bookID)
        {
            DB.Books.DeleteBook(bookID);
        }
    }
}
