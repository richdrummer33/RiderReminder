using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Model : MonoBehaviour
{


    private int camWidth;
    private int camHeight;
    private int xOffScreen;
    private int yOffScreen;
    private int blockWidth;
    private int blockHeight;
    private int blockSize;
    private int xBlocks;
    private int yBlocks;
    private int xOff;
    private int yOff;


    public RawImage videoImage;
    public RawImage displayImage;
    private WebCamTexture webcamTexture;
    public Texture2D clearBoxTexture;
    public Texture2D activeBoxTexture;
    public Color[] blockData;
    private Cell[,] Cells;
    public GameObject OverlayObj;
    float startThresold;

    public void ChangeThreshold(float newValue)
    {
        Debug.Log("NEw thres setting " + startThresold * 2f * newValue);
        Cell.Threshold = startThresold * 2f * newValue;  // newValue * blockSize;
    }

    void Start()
    {
        StartCoroutine(Report());
        startThresold = Cell.Threshold;
        OverlayObj = GameObject.Find("Overlay");
        blockWidth = 2;
        blockHeight = 2;
        blockSize = blockWidth * blockHeight;

        //webcamTexture = new WebCamTexture(1280,720,30);
        webcamTexture = new WebCamTexture(640, 360, 30);
        //webcamTexture = new WebCamTexture(640, 480, 30);


        videoImage.texture = webcamTexture;
        videoImage.material.mainTexture = webcamTexture;
        webcamTexture.Play();

        camWidth = webcamTexture.width;
        camHeight = webcamTexture.height;
        xOffScreen = (int)((OverlayObj.GetComponent<RectTransform>().rect.width - camWidth) / 2);
        yOffScreen = (int)((OverlayObj.GetComponent<RectTransform>().rect.height - camHeight) / 2);
        xBlocks = (int)(camWidth / blockWidth);
        yBlocks = (int)(camHeight / blockHeight);
        Cells = new Cell[xBlocks, yBlocks];

        for (int y = 0; y < yBlocks; y++)
        {
            for (int x = 0; x < xBlocks; x++)
            {
                Cells[x, y] = new Cell(x * blockWidth + blockWidth, camHeight - blockHeight - (y * blockHeight));
            }
        }

        blockData = new Color[blockSize];
    }

    /*
    void OnGUI()
    {
        GUI.skin.box = GUIStyle.none;
        GUI.BeginGroup(new Rect(xOffScreen, yOffScreen, camWidth, camHeight));
        for (int y = 0; y < yBlocks; y++)
        {
            for (int x = 0; x < xBlocks; x++)
            {
                if (Cells[x, y].Active)
                {
                    GUI.Box(new Rect(Cells[x, y].x, Cells[x, y].y, blockWidth, blockHeight), activeBoxTexture);
                }
            }
        }
        GUI.EndGroup();
    }
    */

    bool newDetection;
    IEnumerator DetectTimer()
    {
        newDetection = true;
        yield return new WaitForSeconds(1f);
        newDetection = false;
    }

    void Update()
    {
        if (webcamTexture.didUpdateThisFrame)
        {
            diff = 0f;

            for (int y = 0; y < yBlocks; y++)
            {
                for (int x = 0; x < xBlocks; x++)
                {
                    blockData = webcamTexture.GetPixels(x * blockWidth + xOff, y * blockHeight + yOff, blockWidth, blockHeight);
                    float dataR = 0;
                    float dataG = 0;
                    float dataB = 0;

                    foreach (Color nextColor in blockData)
                    {
                        dataR += nextColor.r;
                        dataG += nextColor.g;
                        dataB += nextColor.b;
                    }

                    bool wasDetected = Cells[x, y].CheckCell(dataR + dataG + dataB / 3);

                    if(Cells[x, y].diff > diff)
                    {
                        diff = Cells[x, y].diff;
                    }

                    if (wasDetected)
                    {
                        Debug.Log("Movement detected! At time " + Time.time);
                        if (newDetection == false)
                        {
                            Debug.Log("NEW Movement detected! At time " + Time.time);
                            StartCoroutine(DetectTimer());
                        }
                    }
                }
            }
        }
    }

    float diff;

    IEnumerator Report()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);

            Debug.Log("max diff = " + diff + " Thres = " + Cell.Threshold);
        }
    }
}
