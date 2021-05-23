<h1>TCahe</h1>

The TCahce class provides a simple generic wrapper around the .NET ConcurrentDictionary to enable stored values to become stale. Once stale the values are removed from the cache. Note that this adds a performance overhead when accessing the dictionary.

<h2>TCache.cs File</h2>
The generic wrapper is contained within the "TCache\TCache\TCache.cs" file which is the only file required.

<h2>Staleness</h2>
An item becomes state by default after 12 hours though this can be changed by setting the StaleDataPeriod property.

<h2>Example Usage</h2>

  TCache<string, string> curCache = new TCache<string, string>();
  
  // value becomes stale after 60 seconds
  
  curCache.StaleDataPeriod = 60;   
  
  curCache["A"] ??= "10";
  
  var currValue = Int32.Parse(curCache["A"]);

