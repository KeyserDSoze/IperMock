// See https://aka.ms/new-console-template for more information
using IperMock;
using IperMock.Console;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.Security.Principal;

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

var firstMock = new Mock<Fudlish>();
firstMock.Set(x => x.A, "dasdsa")
    .Set(x => x.B, "aaaaa");
var secondMock = new Mock<ISoldi>();
secondMock.Set(x => x.A, "ddddd");
var q = firstMock.Instance;
Console.WriteLine(firstMock.Instance.A);
Console.WriteLine(firstMock.Instance.B);
Console.WriteLine(secondMock.Instance.A);

var listMock = new Mock<IEnumerable<string>>();
listMock.Add(x => x, "aaaaa");
//var list = listMock.Instance.Where(x => x == string.Empty).ToList();

var identityContext = new Mock<IIdentity>();
identityContext.Set(x => x.Name, "Costacurta");

var context = new Mock<HttpContext>();
context
    .Default(x => x.User.Identities)
    .Default(x => x.User.Identity)
    .Add(x => x.User.Identities, new System.Security.Claims.ClaimsIdentity(identityContext.Instance))
        .Set(x => x.User.Identity.Name, "Lisandro");
context
        .Set(x => x.Request.Headers.UserAgent, new StringValues("xasdsadsa"));
var giraf = context.Instance;
var identity = context.Instance.User.Identity;
Console.WriteLine(giraf.User.Identity.Name);
Console.WriteLine(identity.Name);
Console.WriteLine(giraf.Request.Headers.UserAgent);






