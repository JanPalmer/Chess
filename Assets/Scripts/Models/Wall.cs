using System.Collections.Generic;
using Models;
using UnityEngine;

namespace Models
{
    public class Wall : Chessman
    {
        public override List<PossibleMove> GetPossibleMoves()
        {
            return new List<PossibleMove>();
        }
    }

}
