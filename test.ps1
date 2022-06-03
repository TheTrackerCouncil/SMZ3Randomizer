$json = gc D:\Source\SMZ3Randomizer\src\Randomizer.SMZ3.Tracking\locations.json -Raw | ConvertFrom-Json 

foreach ($location in $json.Locations)
{
    $name = $location.Name[0]
    $id = $location.Id
    if ($id -ge 256) {
        $id = "256 + " + ($id - 256)
    }

    $intAddress = $location.MemoryAddress;
    $intFlag = $location.MemoryFlag

    $address = '{0:X}' -f $intAddress
    $flag = '{0:X}' -f $intFlag
    echo "$name ($id) - 0x$address, 0x$flag"
}