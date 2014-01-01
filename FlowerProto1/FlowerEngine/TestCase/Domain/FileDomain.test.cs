using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.EntityClient;
using NUnit.Framework;
using FlowerEngine;


namespace FlowerEngine.Domain
{
    [TestFixture]
    partial class FileDomain
    {
        [SetUp]
        public void ＤＢを一旦クリアする()
        {
            using (var entities = new Model.FlowerEntities())
            {
                foreach (var file in entities.Files)
                {
                    entities.Files.Remove(file);
                }
                entities.SaveChanges();
            }
            return;
        }

        [SetUp]
        public void ファイルを一旦クリア()
        {
            var files = System.IO.Directory.GetFiles(@".\TestCase\FileStore\", "*.dat");

            foreach (var file in files)
            {
                System.IO.File.Delete(file);
            }
            return;
        }


        [Test]
        public void ID取得_レコードなし()
        {
            var domain = new FileDomain(@".\TestCase\FileStore\");

            Assert.True(domain.getId() == "1");
            return;
        }

        [Test]
        public void ID取得_レコードあり()
        {
            var domain = new FileDomain(@".\TestCase\FileStore\");

            Model.File file = null;
            file = domain.Regist2DB("Test_Text.txt", "1");
            file = domain.Regist2DB("Test_Text.txt", "2");
            Assert.True(domain.getId() == "3");
            return;
        }



        [Test]
        public void ファイル格納_1()
        {
            var domain      = new FileDomain(@".\TestCase\FileStore\");
            var filepath    = @".\TestCase\Data\Test_Text.txt";
            var id          = "1";

            domain.storeFile(filepath,id);
            Assert.True(System.IO.File.Exists(@".\TestCase\FileStore\1.dat") == true);
            return;
        }

        [Test]
        public void ファイル格納_2()
        {
            var domain = new FileDomain(@".\TestCase\FileStore\");
            var filepath = @".\TestCase\Data\Test_Text.txt";
            var id = "2";

            domain.storeFile(filepath, id);
            Assert.True(System.IO.File.Exists(@".\TestCase\FileStore\2.dat") == true);
            return;
        }





        [Test]
        public void ファイルDB登録()
        {
            var domain = new FileDomain(@".\TestCase\FileStore\");

            var file = domain.Regist2DB("Test_Text.txt", "TestId");

            Assert.True(file != null);
            using (var entities = new Model.FlowerEntities())
            {
                var latestileId = (from e in entities.Files.AsEnumerable() select e.ID).Max();
                Assert.True(file.ID == latestileId);
            }
            return;
        }

        [Test]
        public void ファイルDB登録_連続登録()
        {
            var domain = new FileDomain(@".\TestCase\FileStore\");

            var file = domain.Regist2DB("Test_Text.txt", "TestId01");
                file = domain.Regist2DB("Test_Text.txt", "TestId02");

            Assert.True(file != null);
            using (var entities = new Model.FlowerEntities())
            {
                var cnt = (from e in entities.Files.AsEnumerable() where e.Name == "Test_Text.txt" select e).Count();
                Assert.True(cnt == 2);
            }
            return;
        }





        [Test]
        [ExpectedException(typeof(Exception))]
        public void ファイル登録_ファイルなし()
        {
            var domain = new FileDomain();

            domain.Regist(@".\nothing.txt");
            return;
        }


        [Test]
        public void ファイル登録()
        {
            var domain = new FileDomain(@".\TestCase\FileStore\");

            var newId = domain.Regist(@"TestCase\Data\Test_Text.txt");
            using (var entities = new Model.FlowerEntities())
            {
                var latestFileId = (from e in entities.Files.AsEnumerable() select e.ID).Max();
                Assert.True(latestFileId == "1");
                Assert.True(newId        == "1");
                Assert.True(System.IO.File.Exists(@".\TestCase\FileStore\1.dat") == true);
            }
        }

        [Test]
        public void ファイル登録_連続()
        {
            var domain = new FileDomain(@".\TestCase\FileStore\");

            domain.Regist(@"TestCase\Data\Test_Text.txt");
            domain.Regist(@"TestCase\Data\Test_Text.txt");
            using (var entities = new Model.FlowerEntities())
            {
                var latestFileId = (from e in entities.Files.AsEnumerable() select e.ID).Max();
                Assert.True(latestFileId == "2");
            }
            Assert.True(System.IO.File.Exists(@".\TestCase\FileStore\1.dat") == true);
            Assert.True(System.IO.File.Exists(@".\TestCase\FileStore\2.dat") == true);
        }


        [Test]
        public void ファイル削除()
        {
            var domain = new FileDomain(@".\TestCase\FileStore\");

            var newId = domain.Regist(@"TestCase\Data\Test_Text.txt");
            domain.removeFile(newId);
            Assert.True(System.IO.File.Exists(@".\TestCase\FileStore\1.dat") == false);
            return; 
        }

        [Test]
        public void ファイル削除DB()
        {
            var domain = new FileDomain(@".\TestCase\FileStore\");

            var newId = domain.Regist(@"TestCase\Data\Test_Text.txt");
            domain.removeFromDB(newId, null);
            using (var entities = new Model.FlowerEntities())
            {
                Assert.True(entities.Files.Count() == 0);
            }
            return;
        }

        [Test]
        public void ファイル削除_１つ()
        {
            var domain = new FileDomain(@".\TestCase\FileStore\");
            var newId = domain.Regist(@"TestCase\Data\Test_Text.txt");

           var file =  domain.Remove(newId);

            Assert.True(System.IO.File.Exists(@".\TestCase\FileStore\1.dat") == false);
            using (var entities = new Model.FlowerEntities())
            {
                Assert.True(entities.Files.Count() == 0);
            }

            Assert.True(file.ID == "1");
            return;
        }
    }
}
