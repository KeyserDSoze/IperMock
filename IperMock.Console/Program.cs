// See https://aka.ms/new-console-template for more information
using IperMock;
using IperMock.Console;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

Console.WriteLine("Hello, World!");

//var mock = new Mock<Falling>();
//mock
//    .Set(x => x.A, "SSSS")
//    .Set(x => x.B, "")
//    .Set(x => x.Selius.X, 4)
//    .Set(x => x.Selius.Fulvius.C, 5)
//        .Construct(x => x.Selius.Fai.Fano, "calcutta", "salumificio")
//    .Set(x => x.Selius.Foligno.Cocco, "Alfio")
//        .Construct(x => x.Mari, default);

//var dela = mock.Instance;
//var sabri = mock.Instance.Selius;
//Console.WriteLine(dela.A);
//Console.WriteLine(mock.Instance.Selius.X);
//Console.WriteLine(sabri.X);
//Console.WriteLine(dela.Selius.X);
//Console.WriteLine(dela.Selius.Fai.Fano.Name);
//Console.WriteLine(dela.Selius.Fai.Fano.Surname);
//Console.WriteLine(dela.Selius.Fulvius.C);
//Console.WriteLine(dela.Selius.Foligno.Cocco);


var context = new Mock<HttpContext>();
context
    //.Default(x => x.User.Identity)
    //.Set(x => x.User.Identity.Name, "Lisandro")
    .Set(x => x.Request.Headers.UserAgent, new StringValues("xasdsadsa"));
var giraf = context.Instance;
var identity = context.Instance.User.Identity;
Console.WriteLine(giraf.User.Identity.Name);
Console.WriteLine(identity.Name);
Console.WriteLine(giraf.Request.Headers.UserAgent);






