using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

public class PlaceNode
{
    public int PlaceNodeID { get; set; }
    public string PlaceName { get; set; }
    public string StateName { get; set; }
    public string link { get; set; }
    public int numberOfStatesThatHaveThisPlace { get; set; }
    [NotMapped]
    public List<string> StatesThatHaveThisPlace = new List<string>();

}
