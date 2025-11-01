using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using UWUVCI_AIO_WPF.Helpers;

namespace UWUVCI_MSTest
{
    [TestClass]
    public class ImagePathResolverTests
    {
        private string _tempDir;

        [TestInitialize]
        public void Setup()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), "uwuvci_tests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDir);
        }

        [TestCleanup]
        public void Teardown()
        {
            try { Directory.Delete(_tempDir, recursive: true); } catch { }
        }

        [TestMethod]
        public void ResolveExistingPng_ExactPng_Works()
        {
            var p = Path.Combine(_tempDir, "icon.png");
            File.WriteAllText(p, string.Empty);
            var r = ImagePathResolver.ResolveExistingPng(p);
            Assert.AreEqual(p, r);
        }

        [TestMethod]
        public void ResolveExistingPng_TgaWithSiblingPng_PicksPng()
        {
            var tga = Path.Combine(_tempDir, "bootTvTex.tga");
            var png = Path.Combine(_tempDir, "bootTvTex.png");
            File.WriteAllText(tga, string.Empty);
            File.WriteAllText(png, string.Empty);
            var r = ImagePathResolver.ResolveExistingPng(tga);
            Assert.AreEqual(png, r);
        }

        [TestMethod]
        public void ResolveExistingPng_NoPng_ReturnsNull()
        {
            var tga = Path.Combine(_tempDir, "bootTvTex.tga");
            File.WriteAllText(tga, string.Empty);
            var r = ImagePathResolver.ResolveExistingPng(tga);
            Assert.IsNull(r);
        }

        [TestMethod]
        public void ResolveFromPeers_FindsCanonicalNextToPeer()
        {
            var dir = Path.Combine(_tempDir, "createdIMG");
            Directory.CreateDirectory(dir);
            var icon = Path.Combine(dir, "iconTex.png");
            File.WriteAllText(icon, string.Empty);
            var r = ImagePathResolver.ResolveFromPeers("iconTex", new [] { dir });
            Assert.AreEqual(icon, r);
        }

        [TestMethod]
        public void NormalizeAll_ResolvesPeersAndSiblings()
        {
            var dir = Path.Combine(_tempDir, "createdIMG");
            Directory.CreateDirectory(dir);
            var tv = Path.Combine(dir, "bootTvTex.png");
            var icon = Path.Combine(dir, "iconTex.png");
            File.WriteAllText(tv, string.Empty);
            File.WriteAllText(icon, string.Empty);

            string[] paths = { null, tv, null, null };
            string[] keys = { "iconTex", "bootTvTex", "bootDrcTex", "bootLogoTex" };
            var result = ImagePathResolver.NormalizeAll(paths, keys);

            Assert.AreEqual(icon, result[0]);
            Assert.AreEqual(tv, result[1]);
        }
    }
}

