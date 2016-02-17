togglez
=======
[![Build Status](https://travis-ci.org/adron-orange/togglez.svg?branch=master)](https://travis-ci.org/adron-orange/togglez)

Feature toggling for .NET. Uses Zookeeper.

## build instructions

git clone git@github.com:heartysoft/togglez.git
./build.sh on OS-X/Linux
./build.cmd on Windows

## features

* Use settings stored as JSON strings in Zookeeper.
* Use values in Zookeeper with simple Get<>() methods.
* Subscribe to changes; your handler gets called whe1n the Zookeeper value is updated.
* Option to wait synchronously until settings are first fetched. This can be useful in bootstrapping scenarios.
* Uses Zookeeper underneath the hood.
* Takes care of disconnects, reconnects, etc.
* Nuget package IlMerges dependencies. The Zookeeper .NET uses log4net, and as such, easily leads to dll hell scenarios. This nuget package IlMerges and internalizes the Zookeper client, log4net and JSON.NET to avoid annoying versioning issues.

## Usage

Using Togglez is straightforward. The following code sets things up:

```
var zk = ZkRunner.New()
                .Path(() => "/path/in/zookeeper")
                .ConnectionString(() => "192.168.60.2:2181,192.168.60.3:2181,192.168.60.4:2181")
                .SessionTimeout(() => TimeSpan.FromSeconds(3))
                .Build();

var togglez = zk.Start();
```

Note: The setting "/path/in/zookeeper" must be a json string.

The previous code starts off a subscription. You can use the togglez object to read in already fetched values. You can also use it to subscribe to changes. 

### Reading Values

You can use a togglez object to fetch values. The syntax for this is as follows:

```
togglez.Get<int>("foo")
```

This reads the "foo" property of the json document specified at the Zookeeper path. 

If the settings at "path" haven't been received yet, reading the value will give you the default value for that type. *Get<>() doesn't query Zookeeper, rather it reads values already fetched from Zookeeper. * If the first fetch hasn't finished, you're not really reading values in Zookeeper at all. For this reason, there's a convenience method:

```
togglez.WaitForFirstSettings(TimeSpan.FromSeconds(10));
```
This will wait synchronously for first time settings. If you intend to use Get<>() (instead of subscribe), be sure to wait for first time settings at the bootstrapping stage. The recommended usage though, is subscribing.

### Subscribing

Subscribing is also straight forward:

```
togglez.SubscribeOn<int>("foo", x => {/*x has the value of foo */});
```

Subscriptions are triggered when the value changes in Zookeeper, or after initial setup. In other words, you don't need to do gets for first time, and subscriptions for changes...simply subscribing will mean your handler gets triggered when the value is first ready, and whenever it changes. Also, because subscriptions work via callbacks, there's no need to "WaitForFirstSettings" when using them.

### One Cool Trick
Togglez settings are stored in Zookeeper as json strings. This means that all the "settings" in that document are collectively atomic; in fact, this is the reason for using json as opposed to individual settings in Zookeeper. You can use this atomicity to your advantage. If you subscribe to a property on the target json, when that value is ready, the whole document will be ready. Togglez objects are threadsafe. So, you can easily do something like this:

```
var togglez = zk.Start();
togglez.SubscribeOn<int>("foo", x => {
   /*x has the value of foo */
  var bar = togglez.Get<string>("bar");
  //use bar
});
```
This allows you to group multiple things in a handler, instead of creating lots of handlers.

## Disposing 
The runner (zk above) is disposable. You'd typically create runners at bootstrapping and dispose them when shutting down.
