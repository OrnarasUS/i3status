using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Net.Sockets;

string NetworkInfo(string ifname) {
    try
    {
    var host = NetworkInterface.GetAllNetworkInterfaces();    
    var netface = host.First(i => i.Name == ifname);
    var address = netface.GetIPProperties().UnicastAddresses
        .First(i => i.Address.AddressFamily == AddressFamily.InterNetwork);
    var text = $"IP: {address.Address}";
        switch (netface.OperationalStatus)
        {
        case OperationalStatus.Up:
            return $"{{\"full_text\":\"IP: {address.Address}\",\"color\":\"#32CD32\"}}";
        case OperationalStatus.Down:
        case OperationalStatus.LowerLayerDown:
            return "{\"full_text\":\"{down}\",\"color\":\"#FF0000\"}";
        case OperationalStatus.NotPresent:
            return "{\"full_text\":\"-\",\"color\":\"#FF0000\"}";
        case OperationalStatus.Dormant:
            return "{\"full_text\":\"{sleep}\",\"color\":\"#FFFF00\"}";
        default:
            return "{\"full_text\":\"???\",\"color\":\"#FFFF00\"}";
    }
}
    catch
    {
        return "{\"full_text\":\"{down}\",\"color\":\"#FF0000\"}";
    }
}
string Time() =>
    $"{{\"full_text\":\"{DateTime.Now:HH:mm:ss}\"}}";
string Language(){
    var args = new ProcessStartInfo("xkblayout-state", "print %s"){
        RedirectStandardOutput = true
    };
    var proc = new Process() {
        StartInfo = args
    };
    proc.Start();
    proc.WaitForExit();
    var o = proc.StandardOutput.ReadToEnd();
    
    return $"{{\"full_text\":\"{o.ToUpper()}\"}}";
}
string CPUTemp(){
    var text = File.ReadAllText("/sys/class/thermal/thermal_zone0/temp");
    var temp = int.Parse(text) / 1000;
    var res = $"{{\"full_text\": \"{temp:f0} °C\"";
    res += temp switch{
        < 50 => "}",
        >= 65 => ", \"color\": \"#FF0000\"}",
        _ => ", \"color\": \"#FFFF00\"}"
    };
    return res;
}
string Battery(byte idBAT = 0){
    var text = File.ReadAllText($"/sys/class/power_supply/BAT{idBAT}/capacity");
    var charging = File.ReadAllText($"/sys/class/power_supply/BAT{idBAT}/status").StartsWith("Charging");
    var bat = int.Parse(text);
    return $"{{\"full_text\": \"{(charging?"⚡":"🔋")} {bat}\", \"min_width\": \"🔋 100\", \"align\": \"center\"{(bat <= 15?", \"color\":\"#FFFF00\"":"")}}}";
}


Console.WriteLine("{\"version\": 1}");
Console.WriteLine("[");
while (true){
    var text = "[";    
    text += $"{CPUTemp()},";
    text += $"{Language()},";
    text += $"{NetworkInfo("wlp59s0")},";
    text += $"{Battery()},";
    text += $"{Time()}";
    text += "],";    
    Console.WriteLine(text);
    Thread.Sleep(250);
}