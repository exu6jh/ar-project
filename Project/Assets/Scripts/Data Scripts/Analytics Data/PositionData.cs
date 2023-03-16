using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PositionData
{
  public XYCord StartPos;

  public List<ManipulationData> InBetween;

  public XYCord EndPos;
}
