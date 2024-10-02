using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoggerTest : MonoBehaviour
{
    public LogType lineType;
    public TMP_InputField _inputField;
    [SerializeField] private Button _testButton; 
    private Logger _logger;
    void Start()
    {   
        _logger = GetComponent<Logger>();
        _testButton.onClick.AddListener(() => {
            if (string.IsNullOrEmpty(_inputField.text)) _logger.Log("This is a test message", lineType);
            else _logger.Log(_inputField.text, lineType);
            }
        );
    }
}
