using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class StateEntity
{
    public int StateEntityID { get; set; }
    public string StateName { get; set; }
    public ICollection<PlaceNodeEntity> PlaceNodes { get; set; }
}