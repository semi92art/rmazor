using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Path<Tile> : IEnumerable<Tile>
{
    public Tile LastStep { get; private set; }
    public Path<Tile> PreviousSteps { get; private set; }
    public double TotalCost { get; private set; }

    private Path(Tile _LastStep, Path<Tile> _PreviousSteps, double _TotalCost)
    {
        LastStep = _LastStep;
        PreviousSteps = _PreviousSteps;
        TotalCost = _TotalCost;
    }

    public Path(Tile _Start) : this(_Start, null, 0) { }

    public Path<Tile> AddStep(Tile _Step, double _StepCost)
    {
        return new Path<Tile>(_Step, this, TotalCost + _StepCost);
    }

    public IEnumerator<Tile> GetEnumerator()
    {
        for (var p = this; p != null; p = p.PreviousSteps)
            yield return p.LastStep;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    
}
