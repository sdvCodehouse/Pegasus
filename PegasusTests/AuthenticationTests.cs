﻿using NUnit.Framework;

namespace PegasusTests
{
    public class AuthenticationTests
    {
        [Test]
        public void CallingAuthenticate()
        {
            Assert.Pass("Need to implement Authentication Testing");
            //var myJsonConfig = "{\"apiRoot\": \"https://pegasus2api.local\" }";
            //var stream = new MemoryStream(Encoding.UTF8.GetBytes(myJsonConfig));

            //var configuration = new ConfigurationBuilder()
            //   .AddJsonStream(stream)
            //   .Build();

            ////TODO Urgent!! Change this to use a mock authentication rather than real credentials
            //var apiHelper = new ApiHelper(configuration);
            //var credentials = new UserCredentials {Username = "simon.davall@gmail.com", Password = "H3r3ford!!"};
            //var sut = apiHelper.Authenticate(credentials).Result;

            //Assert.IsNotNull(sut);
            //Assert.AreEqual(credentials.Username, sut.Username);
        }
    }
}
