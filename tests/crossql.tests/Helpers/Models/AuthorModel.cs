using System.Collections.Generic;
using System.Linq;
using crossql.Attributes;

namespace crossql.tests.Helpers.Models
{
    public class AuthorModel : ModelBase
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }

        [Ignore]
        public bool IsCoolName => FirstName.Contains("e");

        [ManyToMany]
        public List<BookModel> Books { get; set; } = new List<BookModel>();

        public void AddBooks(params BookModel[] books)
        {
            Books.AddRange(books);
        }

        public void RemoveBooks(params BookModel[] books)
        {
            Books.RemoveAll(books.Contains);
        }
    }
}