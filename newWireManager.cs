using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ColorWire
{
    public Color color = new(255, 0, 205, 1.0f);
    public bool IsFree = true;

    public ColorWire() { }
    public ColorWire(Color c)
    {
        color = c;
        IsFree = true;
    }
    public static void BusyReset(ref ColorWire[] cw)
    {
        for (int i = 0; i < cw.Length; i++)
        {
            cw[i].IsFree = true;
        }
    }
    public static Color GetRandomColor(List<Color> array)
    {
        Color comparing = new(0,0,0,0);
        for (int i = 0; i < array.Count; i++)
        {
            int index = 0;
            index = Random.Range(0, array.Count);

            return array[index];
        }
        return new();
    }
}

public class newWireManager : MonoBehaviour
{
    [SerializeField] private Wire[] _wire;
    [SerializeField] private Color[] _colorSource;
    [SerializeField] private float[] _axeY;
    private List<Color> _color;

    void Start()
    {
        _color = new();

        for (int i = 0; i < _colorSource.Length; i++)
        {
            _color.Add(_colorSource[i]);
        }

        _axeY = new float[_wire.Length];
        for (int i = 0; i < _wire.Length; i++)
        {
            //_axeY[i] = _wire[i]._rtr.anchoredPosition.y;
            //_wire[i].SetColor(_color[i]);
            _wire[i].SetColorWanderingAndLine(_colorSource[i]);
            _wire[i].SetColorAnchor(ColorWire.GetRandomColor(_color));
            //Debug.Log(_wire[i]._rtr.anchoredPosition.y);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }   
}