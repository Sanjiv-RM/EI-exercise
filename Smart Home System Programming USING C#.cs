// IDevice interface for common device operations
public interface IDevice
{
    int Id { get; }
    string Type { get; }
    string Status { get; set; }
    void TurnOn();
    void TurnOff();
    string GetStatus();
}

// Observer interface for the Observer Pattern
public interface IObserver
{
    void Update();
}

// Subject interface for the Observer Pattern
public interface ISubject
{
    void RegisterObserver(IObserver observer);
    void RemoveObserver(IObserver observer);
    void NotifyObservers();
}

// Base class for a device
public abstract class Device : IDevice
{
    public int Id { get; private set; }
    public string Type { get; private set; }
    public string Status { get; set; } = "off";

    protected Device(int id, string type)
    {
        Id = id;
        Type = type;
    }

    public abstract void TurnOn();
    public abstract void TurnOff();

    public string GetStatus()
    {
        return $"{Type} {Id} is {Status}.";
    }
}


public class Light : Device
{
    public Light(int id) : base(id, "Light") { }

    public override void TurnOn()
    {
        Status = "on";
    }

    public override void TurnOff()
    {
        Status = "off";
    }
}

public class Thermostat : Device
{
    public int Temperature { get; set; }

    public Thermostat(int id, int temperature) : base(id, "Thermostat")
    {
        Temperature = temperature;
    }

    public override void TurnOn()
    {
        Status = "on";
    }

    public override void TurnOff()
    {
        Status = "off";
    }
}

public class DoorLock : Device
{
    public DoorLock(int id) : base(id, "Door") { }

    public override void TurnOn()
    {
        Status = "locked";
    }

    public override void TurnOff()
    {
        Status = "unlocked";
    }
}


public static class DeviceFactory
{
    public static IDevice CreateDevice(int id, string type, int? temperature = null)
    {
        switch (type.ToLower())
        {
            case "light":
                return new Light(id);
            case "thermostat":
                return new Thermostat(id, temperature ?? 70);
            case "door":
                return new DoorLock(id);
            default:
                throw new ArgumentException("Invalid device type.");
        }
    }
}


using System;
using System.Collections.Generic;
using System.Linq;

public class SmartHomeHub : ISubject
{
    private readonly List<IDevice> _devices = new List<IDevice>();
    private readonly List<Schedule> _schedules = new List<Schedule>();
    private readonly List<Trigger> _triggers = new List<Trigger>();
    private readonly List<IObserver> _observers = new List<IObserver>();

    public void AddDevice(IDevice device)
    {
        _devices.Add(device);
        NotifyObservers();
    }

    public void RemoveDevice(int id)
    {
        var device = _devices.FirstOrDefault(d => d.Id == id);
        if (device != null)
        {
            _devices.Remove(device);
            NotifyObservers();
        }
    }

    public void SetSchedule(int deviceId, TimeSpan time, string command)
    {
        _schedules.Add(new Schedule { DeviceId = deviceId, Time = time, Command = command });
    }

    public void AddTrigger(string conditionType, string conditionOperator, int conditionValue, string action)
    {
        _triggers.Add(new Trigger { ConditionType = conditionType, Operator = conditionOperator, Value = conditionValue, Action = action });
    }

    public void TurnOnDevice(int id)
    {
        var device = _devices.FirstOrDefault(d => d.Id == id);
        device?.TurnOn();
        NotifyObservers();
    }

    public void TurnOffDevice(int id)
    {
        var device = _devices.FirstOrDefault(d => d.Id == id);
        device?.TurnOff();
        NotifyObservers();
    }

    public void CheckSchedules(DateTime currentTime)
    {
        foreach (var schedule in _schedules)
        {
            if (currentTime.TimeOfDay >= schedule.Time)
            {
                var device = _devices.FirstOrDefault(d => d.Id == schedule.DeviceId);
                if (device != null)
                {
                    if (schedule.Command == "Turn On")
                        device.TurnOn();
                    else if (schedule.Command == "Turn Off")
                        device.TurnOff();
                }
            }
        }
        NotifyObservers();
    }

    public void CheckTriggers()
    {
        foreach (var trigger in _triggers)
        {
            // Placeholder for trigger checking logic
        }
        NotifyObservers();
    }

    public void RegisterObserver(IObserver observer)
    {
        _observers.Add(observer);
    }

    public void RemoveObserver(IObserver observer)
    {
        _observers.Remove(observer);
    }

    public void NotifyObservers()
    {
        foreach (var observer in _observers)
        {
            observer.Update();
        }
    }

    public string GetStatusReport()
    {
        return string.Join(" ", _devices.Select(d => d.GetStatus()));
    }

    public string GetScheduledTasks()
    {
        return string.Join(", ", _schedules.Select(s => $"{{device: {s.DeviceId}, time: \"{s.Time}\", command: \"{s.Command}\"}}"));
    }

    public string GetAutomatedTriggers()
    {
        return string.Join(", ", _triggers.Select(t => $"{{condition: \"{t.ConditionType} {t.Operator} {t.Value}\", action: \"{t.Action}\"}}"));
    }

    private class Schedule
    {
        public int DeviceId { get; set; }
        public TimeSpan Time { get; set; }
        public string Command { get; set; }
    }

    private class Trigger
    {
        public string ConditionType { get; set; }
        public string Operator { get; set; }
        public int Value { get; set; }
        public string Action { get; set; }
    }
}


public class DeviceProxy : IDevice
{
    private readonly IDevice _realDevice;

    public DeviceProxy(IDevice realDevice)
    {
        _realDevice = realDevice;
    }

    public int Id => _realDevice.Id;
    public string Type => _realDevice.Type;
    public string Status
    {
        get => _realDevice.Status;
        set => _realDevice.Status = value;
    }

    public void TurnOn()
    {
        // Add logging or access control here
        _realDevice.TurnOn();
    }

    public void TurnOff()
    {
        // Add logging or access control here
        _realDevice.TurnOff();
    }

    public string GetStatus()
    {
        return _realDevice.GetStatus();
    }
}



class Program
{
    static void Main()
    {
        var hub = new SmartHomeHub();
        
        var light = DeviceFactory.CreateDevice(1, "light");
        var thermostat = DeviceFactory.CreateDevice(2, "thermostat", 70);
        var door = DeviceFactory.CreateDevice(3, "door");

        hub.AddDevice(light);
        hub.AddDevice(thermostat);
        hub.AddDevice(door);

        hub.SetSchedule(2, new TimeSpan(6, 0, 0), "Turn On");
        hub.AddTrigger("temperature", ">", 75, "turnOff(1)");

        Console.WriteLine(hub.GetStatusReport());
        Console.WriteLine(hub.GetScheduledTasks());
        Console.WriteLine(hub.GetAutomatedTriggers());

      
        hub.TurnOnDevice(1);

       
        hub.CheckSchedules(DateTime.Now);

        
        hub.CheckTriggers();
    }
}
