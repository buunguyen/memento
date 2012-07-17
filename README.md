About
=======

Memento is a lightweight and extensible undo/redo framework for .NET applications.
It provide undo/redo support for the following events(*) out of the box:  

* Property change
* Collection element addition
* Collection element removal
* Collection element's index change

While these basic events have proved to be sufficient for most applications I've worked with, 
Memento allows you to build custom events and plug into the framework.

**Note**: *events* here mean those actions that cause changes to the system state, e.g. change
value of a property etc. They are not .NET events.

Getting Started
=======

Install from NuGet
```
Install-Package Memento
```

Create an instance of a `Mementor`. 
```
var mementor = Mementor.Create();
```
**Note**: it's typically sufficient to use one single instance for an entire application. 
But you can create multiple instances as needed and each of them has a different undo/redo
stack. Regardless, make sure you store this instance somewhere easily accessible to other
parts of code.

### Mark Property Changes
```
// Mark change via expression syntax
mementor.PropertyChange(shape, () => shape.Radius);

// Mark change via string name
mementor.PropertyChange(shape, "Radius");

// Mark change with explicit property value (which will be restored to when undo)
mementor.PropertyChange(shape, "Radius", 10);
```

### Mark Collection Changes
```
// Addition
mementor.ElementAdd(screen, shape);

// Removal
mementor.ElementRemove(screen, shape);

// Removal with explicit index (to be restored to when undo)
mementor.ElementRemove(screen, shape, index);

// Index change
mementor.ElementIndexChange(screen, shape);

// Index change with explicit index (to be restored to when undo)
mementor.ElementIndexChange(screen, shape, index);
```
### Perform Undo and Redo
```
// Undo the last event
if (mementor.CanUndo) mementor.Undo();

// Redo a previous undo
if (mementor.CanRedo) mementor.Redo();
```

### Reset

At any point of time, you can reset a `Mementor` to its original state.
```
mementor.Reset();
```

### Batch Marking

If you want to undo multiple events at once, batch them together.
```
// Batch via block
mementor.Batch(() => {
	// change events happen here
});

// Undo all events in the previous batch
mementor.Undo(); 

// Batch explicitly
mementor.BeginBatch();
// ... sometime later
mementor.EndBatch(); 

```

### Disable Marking

If you want to temporarily disable marking (effectively making `Mementor` ignores
all calls to change marking methods like `PropertyChange`), do one of the followings.
```
mementor.ExecuteNoTrack(() => { 
	// changes happened in this block are ignored
});

mementor.IsTrackingEnabled = false;
```

### Event Handling

You can be notified when there is change to the undo/redo stack of a `Mementor` 
by handling its `Changed` event. For example if you call `Undo()`, this event
will be fired with the associated undone event.
```
mementor.Changed += (_, args) => {
	// args allow you to access to the event associated with this notification
}
```

### Custom Mementor Events

You can write your own custom event by extending `Memento.BaseEvent` class.
Then you can use it with a `Mementor` as follows.
```
mementor.MarkEvent(customEvent);
```

### Want to learn more?
The comprehensive test suite for Memento should be a good source of reference.

Contact
=======

* Email: [buunguyen@gmail.com](mailto:buunguyen@gmail.com)
* Blog: [www.buunguyen.net](http://www.buunguyen.net/blog)
* Twitter: [@buunguyen](https://twitter.com/buunguyen/)
