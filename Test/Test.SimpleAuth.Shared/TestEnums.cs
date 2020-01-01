using NUnit.Framework;
using SimpleAuth.Shared.Enums;
using static SimpleAuth.Shared.Enums.Permission;

namespace Test.SimpleAuth.Shared
{
    public class TestEnumPermission
    {
        [Test]
        public void TestGrant()
        {
            Assert.AreEqual(Crud, None.Grant(Add | View | Edit | Delete));
            Assert.AreEqual(Crud, Add.Grant(View | Edit | Delete));
            Assert.AreEqual(Crud, None.Grant(Add, View, Edit, Delete));
            Assert.AreEqual(Crud, Add.Grant(View, Edit, Delete));
            Assert.AreEqual(Crud, Crud.Grant(View, Edit, Delete));
        }

        [Test]
        public void TestRevoke()
        {
            Assert.AreEqual(Crud, Crud.Revoke(None));
            Assert.AreEqual(View | Edit | Delete, Crud.Revoke(Add));
            Assert.AreEqual(Add | Edit | Delete, Crud.Revoke(View));
            Assert.AreEqual(Add | View | Delete, Crud.Revoke(Edit));
            Assert.AreEqual(Add | View |Edit, Crud.Revoke(Delete));
            Assert.AreEqual(Add | View, Crud.Revoke(Edit | Delete));
            Assert.AreEqual(Add | View, Crud.Revoke(Edit, Delete));
            Assert.AreEqual(Add | View, (Add | View | Delete).Revoke(Edit, Delete));
            Assert.AreEqual(Add | View, (Add | View).Revoke(Edit, Delete));
        }
    }
}