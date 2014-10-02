using NUnit.Framework;
using Togglez.Internal;

namespace Togglez.Tests
{
    [TestFixture]
    public class TogglesTests
    {
        [Test]
        public void should_hold_bool_toggle()
        {
            var toggles = new Internal.Togglez();
            toggles.Set(@"{ foo: true, bar:'asd'}");

            Assert.IsTrue(toggles.IsOn("foo"));
        }

        [Test]
        public void should_return_false_for_non_existent_toggle()
        {
            var toggles = new Internal.Togglez();
            toggles.Set(@"{ foo: true, bar:'asd'}");

            Assert.IsFalse(toggles.IsOn("abc"));
        }

        [Test]
        public void should_hold_int()
        {
            var toggles = new Internal.Togglez();
            toggles.Set(@"{ foo: 23, bar:'asd'}");

            Assert.AreEqual(23, toggles.Get<int>("foo"));
        }

        [Test]
        public void should_merge_old_settings_with_new()
        {
            var toggles = new Internal.Togglez();
            toggles.Set(@"{ foo: true, bar:'asd'}");
            toggles.Set(@"{ bam: false, baz:123}");

            Assert.IsTrue(toggles.IsOn("foo"));
            Assert.IsFalse(toggles.IsOn("bam"));
            Assert.AreEqual("asd", toggles.Get<string>("bar"));
            Assert.AreEqual(123, toggles.Get<int>("baz"));
        }

        [Test]
        public void should_replace_old_setting_when_merging()
        {
            var toggles = new Internal.Togglez();
            toggles.Set(@"{ foo: true, bar:'asd'}");
            toggles.Set(@"{ foo: false, baz:123}");

            Assert.IsFalse(toggles.IsOn("foo"));
        }

        [Test]
        public void should_support_factory()
        {
            var toggles = new Internal.Togglez();
            toggles.Set(@"{ foo: true, bar:'asd'}");

            var factory = toggles.GetFactory<bool>("foo");

            Assert.IsTrue(factory());
        }

        [Test]
        public void should_support_notifications()
        {
            var toggles = new Internal.Togglez();
            bool? received = null;

            toggles.SubscribeOn<bool>("foo", x => { received = x; });

            toggles.Set(@"{ foo: true, bar:'asd'}");

            Assert.IsTrue(received.HasValue);
            Assert.IsTrue(received.Value);
        }


        [Test]
        public void should_support_multiple_notifications()
        {
            var toggles = new Internal.Togglez();
            int count = 0;

            toggles.SubscribeOn<bool>("foo", x => { count++; });

            toggles.Set(@"{ foo: true, bar:'asd'}");
            toggles.Set(@"{ foo: false, bar:'asd'}");

            Assert.AreEqual(2, count);
        }

        [Test]
        public void should_only_notify_if_value_has_changed()
        {
            var toggles = new Internal.Togglez();
            int count = 0;

            toggles.SubscribeOn<bool>("foo", x => { count++; });

            toggles.Set(@"{ foo: true, bar:'asd'}");
            toggles.Set(@"{ foo: true, bar:'asd'}");

            Assert.AreEqual(1, count);
        }

        [Test]
        public void should_notify_if_not_null_setting_turns_null()
        {
            var toggles = new Internal.Togglez();
            int count = 0;

            toggles.SubscribeOn<string>("foo", x => { count++; });

            toggles.Set(@"{ foo: 'lala'}");
            toggles.Set(@"{ foo: null}");

            Assert.AreEqual(2, count);
        }


        [Test]
        public void should_not_notify_if_null_remains_null()
        {
            var toggles = new Internal.Togglez();
            int count = 0;

            toggles.SubscribeOn<string>("foo", x => { count++; });

            toggles.Set(@"{ foo: null, bar:2}");
            toggles.Set(@"{ foo: null}");

            Assert.AreEqual(1, count);
        }

        [Test]
        public void should_not_notify_if_value_doesnt_change()
        {
            var toggles = new Internal.Togglez();
            int count = 0;

            toggles.SubscribeOn<string>("foo", x => { count++; });

            toggles.Set(@"{ foo: 'hello'}");
            toggles.Set(@"{ foo: 'hello', bar:25}");

            Assert.AreEqual(1, count);
        }
    }

   
}