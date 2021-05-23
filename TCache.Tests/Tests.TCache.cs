using NUnit.Framework;
using Cache.TCache;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TCache.Tests
{

    public class Tests
    {

        public static void Main(string[] args)
        {
            // Display the number of command line arguments.
            Console.WriteLine(args.Length);
        }

        TCache<SomeDataKey, SomeData> _cache = new TCache<SomeDataKey, SomeData> {
                { new SomeDataKey { firstName="Jack", lastName = "Reacher", dateOfBirth=new DateTime(1982,5,2) },
                    new SomeData { firstName="Jack", lastName = "Reacher", dateOfBirth=new DateTime(1982,5,2),
                    department="HR", jobTitle="Recruiter" } },
                { new SomeDataKey { firstName="Judy", lastName = "Rudy", dateOfBirth=new DateTime(1988,3,12) },
                    new SomeData { firstName="Judy", lastName = "Rudy", dateOfBirth=new DateTime(1988,3,12) ,
                    department="Payroll", jobTitle="Accountant" } },
                { new SomeDataKey { firstName="Simon", lastName = "Smith", dateOfBirth=new DateTime(1990,8,24) },
                    new SomeData { firstName="Simon", lastName = "Smith", dateOfBirth=new DateTime(1990,8,24) ,
                    department="IT", jobTitle="Support" } }
                };

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void test_enumeration()
        {
            var data = new List<SomeData>();
            foreach(var i in _cache)
            {
                data.Add(i.Value);
            }

            Assert.AreEqual(3, data.Count);
        }

        [Test]
        public void test_data_retrieval()
        {
            var keys = new List<SomeDataKey>();
            foreach (var i in _cache)
            {
                keys.Add(i.Key);
            }

            var judy = _cache[new SomeDataKey { firstName = "Judy", lastName = "Rudy", dateOfBirth = new DateTime(1988, 3, 12) }];
            Assert.AreEqual("Judy", judy.firstName);
            Assert.AreEqual(new DateTime(1988, 3, 12), judy.dateOfBirth);
            Assert.AreEqual("Accountant", judy.jobTitle);

            var simon = _cache[new SomeDataKey { firstName = "Simon", lastName = "Smith", dateOfBirth = new DateTime(1990, 8, 24) }];
            Assert.AreEqual("Simon", simon.firstName);
            Assert.AreEqual(new DateTime(1990, 8, 24), simon.dateOfBirth);
            Assert.AreEqual("Support", simon.jobTitle);
        }

        [Test]
        public async Task concurrency()
        {
            TCache<string, string> curCache = new TCache<string, string>();

            var t1 = Task.Run(() => {
                for (int i = 1; i <= 1000; i++)
                {
                    curCache["A"] ??= "0";
                    var currValue = Int32.Parse(curCache["A"]);
                    curCache["A"] = (currValue + 5).ToString();
                    Task.Delay(1);
                }
            });

            var t2 = Task.Run(() => {
                for (int i = 0; i <= 2000; i++)
                {
                    curCache["A"] ??= "0";
                    var currValue = Int32.Parse(curCache["A"]);
                    curCache["A"] = (currValue + 2).ToString();
                    Task.Delay(1);
                }
            });

            await t1;
            await t2;

            var finalValue = Int32.Parse(curCache["A"]);
            Assert.Greater(finalValue,0);
        }

        [Test]
        public async Task test_stale_data_clearing()
        {
            _cache.StaleDataPeriod = 60;

            var judy = _cache[new SomeDataKey { firstName = "Judy", lastName = "Rudy", dateOfBirth = new DateTime(1988, 3, 12) }];

            Assert.IsNotNull(judy);

            await Task.Delay(30000);

            _cache[new SomeDataKey { firstName = "Freddy", lastName = "Wallis", dateOfBirth = new DateTime(1990, 8, 24) }] =
                    new SomeData
                    {
                        firstName = "Freddy",
                        lastName = "Wallis",
                        dateOfBirth = new DateTime(1990, 8, 24),
                        department = "IT",
                        jobTitle = "Support"
                    };

            await Task.Delay(40000);

            judy = _cache[new SomeDataKey { firstName = "Judy", lastName = "Rudy", dateOfBirth = new DateTime(1988, 3, 12) }];
            var fred = _cache[new SomeDataKey { firstName = "Freddy", lastName = "Wallis", dateOfBirth = new DateTime(1990, 8, 24) }];

            Assert.IsNull(judy);
            Assert.IsNotNull(fred);
            Assert.AreEqual("Freddy",fred.firstName);

            await Task.Delay(40000);
            fred = _cache[new SomeDataKey { firstName = "Freddy", lastName = "Wallis", dateOfBirth = new DateTime(1990, 8, 24) }];
            Assert.IsNull(fred);
        }
    }
}