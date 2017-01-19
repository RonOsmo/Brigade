using System;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.ServiceFabric.Actors.Client;
using Osmosys.Abstractions;
using Osmosys.DataContracts;

namespace UnitTestAuthorities
{
    [TestClass]
    public class UnitTestAuthorityActor
    {
        [TestMethod]
        public async Task CreateRootAuthorityAsync()
        {
            var root = new AuthorityDto
            {
                Name = "root",
                AdminRole = "$SYSADMIN$",
                AdminApproverRole = "$SYSADMIN_APPROVER$"
            };

            var authorityProxy = ActorProxy.Create<IAuthority>(new ActorId(root.Path), "http://localhost:19080/");
            root = await authorityProxy.CreateAsync(root);
            Assert.IsNotNull(root);
            Assert.AreEqual(string.Empty, root.Id);
            Assert.AreEqual(string.Empty, root.ContainerId);
            Assert.AreEqual("root", root.Name);
            Assert.AreEqual(".", root.Path);
            Assert.AreEqual("Brigade", root.TeamName);
            Assert.AreEqual("$SYSADMIN$", root.AdminRole);
            Assert.AreEqual("$SYSADMIN_APPROVER$", root.AdminApproverRole);
            Assert.AreEqual(RoleType.Associate.ToString(), root.AssociateRole);
            Assert.AreEqual(RoleType.Captain.ToString(), root.CaptainRole);
            Assert.AreEqual(RoleType.Member.ToString(), root.MemberRole);
            Assert.AreEqual(RoleType.Public.ToString(), root.PublicRole);
            Assert.AreEqual(RoleType.Trainer.ToString(), root.TrainerRole);
        }

        [TestMethod]
        public async Task AddCreateChildAsync()
        {
            var maccy = new AuthorityDto
            {
                Id = "maccy",
                ContainerId = "cfa.vic.gov.au",
                Name = "Macclesfield Fire Brigade"
            };

            var authorityProxy = ActorProxy.Create<IAuthority>(new ActorId("."), "http://localhost:19080/");
            maccy = await authorityProxy.AddCreateChildAsync(maccy);

            Assert.AreEqual("maccy", maccy.Id);
            Assert.AreEqual("cfa.vic.gov.au", maccy.ContainerId);
            Assert.AreEqual("maccy.cfa.vic.gov.au", maccy.Path);
            Assert.AreEqual("Brigade", maccy.TeamName);
            Assert.AreEqual(RoleType.Admin.ToString(), maccy.AdminRole);
            Assert.AreEqual(RoleType.AdminApprover.ToString(), maccy.AdminApproverRole);
            Assert.AreEqual(RoleType.Associate.ToString(), maccy.AssociateRole);
            Assert.AreEqual(RoleType.Captain.ToString(), maccy.CaptainRole);
            Assert.AreEqual(RoleType.Member.ToString(), maccy.MemberRole);
            Assert.AreEqual(RoleType.Public.ToString(), maccy.PublicRole);
            Assert.AreEqual(RoleType.Trainer.ToString(), maccy.TrainerRole);
        }
    }
}
