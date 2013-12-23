using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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


        [Test]
        [ExpectedException(typeof(Exception))]
        public void ファイル登録_ファイルなし()
        {
            var domain = new FileDomain();

            domain.Regist(@".\nothing.txt");
            return ;
        }

        [Test]
        public void ファイルDB登録()
        {
            var domain = new FileDomain(@".\TestCase\FileStore\");

            var file = domain.Regist2DB("Test_Text.txt");

            Assert.True(file != null);
            using (var entities = new Model.FlowerEntities())
            {
                var latestileId = (from e in entities.Files.AsEnumerable() select e.ID).Max();
                Assert.True(file.ID == latestileId);
            }
            return;
        }

        [Test]
        public void ファイル登録()
        {
            var domain = new FileDomain(@".\TestCase\FileStore\");

            domain.Regist(@"TestCase\Data\Test_Text.txt");
            using (var entities = new Model.FlowerEntities())
            {
                var latestFileId = (from e in entities.Files.AsEnumerable() select e.ID).Max();
                Assert.True(latestFileId == "1");
            }

        }

    }
}
