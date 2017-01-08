using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

public class PlaceNodeEntity
{
    public int PlaceNodeEntityID { get; set; }
    public string PlaceName { get; set; }
    public int StateEntityForeignId { get; set; }
    [ForeignKey("StateEntityForeignId")]
    public StateEntity State { get; set; }
}
