using System.Collections.Generic;
using UnityEngine;

// ReSharper disable once CheckNamespace
public class LicensePlates : MonoBehaviour
{
    public List<LicensePlateTag> Plates;
    private string _serialNumber;
    private bool _pickedRandomPlate;
    public string SerialNumber
    {
        get => _serialNumber;
        set
        {
            _serialNumber = value;
            foreach (var plate in Plates)
                plate.SerialNumber = value;

            if (_pickedRandomPlate) return;

            foreach (var plate in Plates)
                plate.gameObject.SetActive(false);

            Plates[Random.Range(0, Plates.Count)].gameObject.SetActive(true);
            _pickedRandomPlate = true;
        }
    }
}

public class LicensePlateTag : MonoBehaviour
{
    public TextMesh LicensePlateLeft;
    public TextMesh LicensPlateRight;
    public int LeftTextLength;

    public string SerialNumber
    {
        set
        {
            if (LeftTextLength >= value.Length)
            {
                LicensePlateLeft.text = value;
                if (LicensPlateRight != null)
                    LicensPlateRight.text = string.Empty;
            }
            else
            {
                LicensePlateLeft.text = value.Substring(0, LeftTextLength);
                LicensPlateRight.text = value.Substring(LeftTextLength);
            }
        }
    }
}