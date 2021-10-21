using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Linq;
using UnityEngine.Networking;
using System.Security.Cryptography;
using UnityEngine.UI;
using System.Text;
using Debug = UnityEngine.Debug;
using System.Globalization;
using TMPro;

public class autoupdate : MonoBehaviour
{
   

    public Process ReadProcess = null;
    private IntPtr handle = IntPtr.Zero;

    public int oldbodys = 0;
    public int olditems = 0;
    public int oldkeys = 0;
    public int whip = 0;
    public int garlic = 0;
    public int laurels = 0;
    public string seed = "";
    public bool sendingdata = false;
    public InputField seedname;
    public InputField usernamefield;

    public Text seednamedisplay;
    public Text statusdisplay;
    public string whip1 = "";
    uint offset = 0;
    uint offset2 = 0;
    public string username = "";
    public TextMeshProUGUI users;
    public void onclick()
    {
        seed = seedname.text;
        seednamedisplay.text = seed;
        username = usernamefield.text;
    }
    public void OpenProcess()
    {

        handle = ProcessMemoryReaderApi.OpenProcess((uint)ProcessMemoryReaderApi.ProcessAccessType.PROCESS_ALL_ACCESS, 0, (uint)ReadProcess.Id);

    }
    public void WriteMemory (IntPtr handle, IntPtr baseaddress, byte[] buffer)
    {

        ProcessMemoryReaderApi.WriteProcessMemory(handle, baseaddress, buffer, (uint)buffer.Length, out IntPtr foo);
    }


    
    public byte[] ReadMemory (IntPtr memoryaddress, uint bytesToRead, out int bytesRead) {
        byte[] buffer = new byte[bytesToRead];
        IntPtr pBytesRead;
        int returned = ProcessMemoryReaderApi.ReadProcessMemory(handle, memoryaddress, buffer, bytesToRead, out pBytesRead);

        bytesRead = pBytesRead.ToInt32();
        return buffer;

    }
    
    IEnumerator getdata()
    {

        //UnityEngine.Debug.Log("running");
        bool changes = false;
        if (ReadProcess != null)
        {
            Process check = Process.GetProcessById(ReadProcess.Id);
            if (check.Id != ReadProcess.Id)
            {
                UnityEngine.Debug.Log("lost process? "+check.Id+" "+ReadProcess.Id);
                statusdisplay.text = "lost process?  ";
                ReadProcess = null;
                handle = IntPtr.Zero;

            }
        }
        

        if (ReadProcess == null && handle == IntPtr.Zero)
        {


            Process[] processes = Process.GetProcessesByName("fceux");
            if (processes.Length == 0 )
            {
                yield break;
            }

            ReadProcess = processes[0];

            if (ReadProcess == null)
            {
                UnityEngine.Debug.Log("TEST null");

                yield break;
            }

            UnityEngine.Debug.Log("found process " + ReadProcess.Id.ToString());
            statusdisplay.text = "found process " + ReadProcess.Id.ToString();

            OpenProcess();
            UnityEngine.Debug.Log("opened process " + ReadProcess.Id.ToString());
            statusdisplay.text = "opened process " + ReadProcess.ProcessName;





        }

        uint baseaddress = 0x003B1388;

        uint paddress = baseaddress;



        offset = BitConverter.ToUInt32( ReadMemory((IntPtr)(paddress + (uint)ReadProcess.Modules[0].BaseAddress), 4, out int bytesread), 0 );
        offset2 = BitConverter.ToUInt32(ReadMemory((IntPtr)(0x36A830 + (uint)ReadProcess.Modules[0].BaseAddress), 4, out bytesread), 0);

        
        byte[] results = ReadMemory((IntPtr)(offset + 0x91), 2, out bytesread);
        if (bytesread == 0)
        {
            statusdisplay.text = "Can't read?";
            handle = IntPtr.Zero;
            ReadProcess = null;
            seed = "";
            seednamedisplay.text = seed;
            yield break;
        }
        if (oldbodys != results[0])
        {
            UnityEngine.Debug.Log("bodies has changed");
            oldbodys = results[0];
            changes = true;
        }

        if (oldkeys != results[1])
        {
            UnityEngine.Debug.Log("Key items have changed");
            oldkeys = results[1];
            changes = true;
        }


        results = ReadMemory((IntPtr)(offset + 0x004A), 1, out bytesread);
        if (results[0] != olditems)
        {
            UnityEngine.Debug.Log("Items have changed");
            olditems = results[0];
            changes = true;
        }


        results = ReadMemory((IntPtr)(offset + 0x004D), 1, out bytesread);
        //UnityEngine.Debug.Log("Garlic: "+results[0].ToString());
        if (results[0] != garlic)
        {
            //controller.curitems["garlic"] = true;
            garlic = results[0];
            changes = true;
        }
        else
        {
           // controller.olditems["garlic"] = false;
        }
        results = ReadMemory((IntPtr)(offset + 0x004C), 1, out bytesread);
        //UnityEngine.Debug.Log("Laurels: " + results[0].ToString());
        if (results[0] != laurels)
        {
           // controller.curitems["laurels"] = true;
           laurels = results[0];
           changes = true;
        }
        else
        {
           // controller.olditems["laurels"] = false;
        }

        results = ReadMemory((IntPtr)(offset + 0x434), 1, out bytesread);
        
        if (( results[0] != whip))
        {
            UnityEngine.Debug.Log("Whip: " + results[0].ToString());
            whip = results[0];
            changes = true;
        }
        
      
        //UnityEngine.Debug.Log(sramoffset + " > " + offset);

        results = ReadMemory((IntPtr)(offset2 + 0x10), 28, out bytesread);
        //UnityEngine.Debug.Log(bytesread + " bytes read: " + ByteArrayToString(results));
        if (! whip1.Equals(ByteArrayToString(results))){
            Debug.Log("new data");
            Debug.Log("Whip upgrade: " + whip1);
            whip1 = ByteArrayToString(results);
            changes = true;
        }
        





        if (changes)
        {
            
            StartCoroutine(sendremotedata());
            changes = false;

            
        }



    }
    void test()
    {

        StartCoroutine(getdata());
    }

    // Start is called before the first frame update
    void Start()
    {

        
        InvokeRepeating("test", 2.0f, 1.0f);
        InvokeRepeating("getremotedata", 2.0f, 1.0f);

    }
    public static string ByteArrayToString(byte[] ba)
    {
        StringBuilder hex = new StringBuilder(ba.Length * 2);
        foreach (byte b in ba)
            hex.AppendFormat("{0:x2}", b);
        return hex.ToString();
    }

    // Update is called once per frame
    void Update()
    {

        
    }
    IEnumerator sendremotedata()
    {
        if (seed.Length <= 0)
        {
            yield break;
        }
        string uri = "http://solforgeladder.com/perl/sendseed.cgi?whip=" + whip + "&bodys=" + oldbodys + "&items=" + olditems + "&keys=" + oldkeys+"&seed="+seed+"&garlic="+garlic+"&laurels="+laurels+"&whip1="+whip1;

        sendingdata = true;
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            if (webRequest.isNetworkError)
            {
                UnityEngine.Debug.Log(pages[page] + ": Error: " + webRequest.error);
                sendingdata = false;
            }
            else
            {
                UnityEngine.Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                //yield return new WaitForSeconds(2);
                sendingdata = false;
            }
        }
    }
    void getremotedata ()
    {
        if (seed.Length <=0)
        {
            //UnityEngine.Debug.Log("no seed");
            return;
        }
        if (sendingdata)
        {
            UnityEngine.Debug.Log("sending can't fetch");
            return;
        }
        StartCoroutine( GetRequest("http://solforgeladder.com/perl/getseed.cgi?username="+username+"&seed=" + seed) );
    }

    IEnumerator GetRequest(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            if (webRequest.isNetworkError)
            {
                UnityEngine.Debug.Log(pages[page] + ": Error: " + webRequest.error);
            }
            else
            {
                if (sendingdata)
                {
                    yield break;
                }
                UnityEngine.Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                parseremote(webRequest.downloadHandler.text);
            }
        }
    }
    void parseremote(string remote)
    {
        string[] foo = remote.Split(',');
        byte[] bytes = new byte[1];
        if (Int32.Parse(foo[0]) != whip )
        {
            UnityEngine.Debug.Log("Whip has changed- remote" + foo[0]);
            //offset + 0x434
            bytes[0] = byte.Parse(foo[0]);
            WriteMemory(handle, (IntPtr)offset + 0x434, bytes);
        }
        if (olditems != Int32.Parse(foo[1]))
        {
            UnityEngine.Debug.Log("Items have changed- remote");
            bytes[0] = byte.Parse(foo[1]);
            WriteMemory(handle, (IntPtr)offset + 0x004A, bytes);
        }
        if (oldkeys != Int32.Parse(foo[2]))
        {
            UnityEngine.Debug.Log("keys have changed- remote");
            bytes[0] = byte.Parse(foo[2]);
            WriteMemory(handle, (IntPtr)offset + 0x92, bytes);
        }
        if (oldbodys != Int32.Parse(foo[3]))
        {
            UnityEngine.Debug.Log("bodys have changed- remote");
            bytes[0] = byte.Parse(foo[3]);
            WriteMemory(handle, (IntPtr)offset + 0x91, bytes);
        }
        if (garlic != Int32.Parse(foo[4]))
        {
            UnityEngine.Debug.Log("garlic have changed- remote");
            bytes[0] = byte.Parse(foo[4]);
            WriteMemory(handle, (IntPtr)offset + 0x004D, bytes);
        }
        if (laurels != Int32.Parse(foo[5]))
        {
            UnityEngine.Debug.Log("Laurels have changed- remote");
            bytes[0] = byte.Parse(foo[5]);
            WriteMemory(handle, (IntPtr)offset + 0x004C, bytes);
        }

        if (whip1 != foo[6].Trim() )
        {
            whip1 = foo[6].Trim();
            WriteMemory(handle, (IntPtr)offset2 + 0x10, ConvertHexStringToByteArray(foo[6].Trim()));

        }
        string[] players = foo[7].Trim().Split('-');
        users.text = String.Join("\n", players);
    }

    public static byte[] ConvertHexStringToByteArray(string hexString)
    {
        if (hexString.Length % 2 != 0)
        {
            throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "The binary key cannot have an odd number of digits: {0}", hexString));
        }

        byte[] data = new byte[hexString.Length / 2];
        for (int index = 0; index < data.Length; index++)
        {
            string byteValue = hexString.Substring(index * 2, 2);
            data[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        }

        return data;
    }



    void updateinventory(byte[] foo)
    {
        
        WriteMemory(handle, (IntPtr)offset + 0x004A, foo);

    }

}



/// <summary>
/// ProcessMemoryReader is a class that enables direct reading a process memory
/// </summary>
class ProcessMemoryReaderApi
{
    // constants information can be found in <winnt.h>
    [Flags]
    public enum ProcessAccessType
    {
        PROCESS_TERMINATE = (0x0001),
        PROCESS_CREATE_THREAD = (0x0002),
        PROCESS_SET_SESSIONID = (0x0004),
        PROCESS_VM_OPERATION = (0x0008),
        PROCESS_VM_READ = (0x0010),
        PROCESS_VM_WRITE = (0x0020),
        PROCESS_DUP_HANDLE = (0x0040),
        PROCESS_CREATE_PROCESS = (0x0080),
        PROCESS_SET_QUOTA = (0x0100),
        PROCESS_SET_INFORMATION = (0x0200),
        PROCESS_QUERY_INFORMATION = (0x0400),
        PROCESS_QUERY_LIMITED_INFORMATION = (0x1000),
        PROCESS_ALL_ACCESS = (0x1F0FFF)

}

// function declarations are found in the MSDN and in <winbase.h>

//		HANDLE OpenProcess(
//			DWORD dwDesiredAccess,  // access flag
//			BOOL bInheritHandle,    // handle inheritance option
//			DWORD dwProcessId       // process identifier
//			);
[DllImport("kernel32.dll")]
    public static extern IntPtr OpenProcess(UInt32 dwDesiredAccess, Int32 bInheritHandle, UInt32 dwProcessId);

    //		BOOL CloseHandle(
    //			HANDLE hObject   // handle to object
    //			);
    [DllImport("kernel32.dll")]
    public static extern Int32 CloseHandle(IntPtr hObject);

    //		BOOL ReadProcessMemory(
    //			HANDLE hProcess,              // handle to the process
    //			LPCVOID lpBaseAddress,        // base of memory area
    //			LPVOID lpBuffer,              // data buffer
    //			SIZE_T nSize,                 // number of bytes to read
    //			SIZE_T * lpNumberOfBytesRead  // number of bytes read
    //			);
    [DllImport("kernel32.dll")]
    public static extern Int32 ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [In, Out] byte[] buffer, UInt32 size, out IntPtr lpNumberOfBytesRead);

    //		BOOL WriteProcessMemory(
    //			HANDLE hProcess,                // handle to process
    //			LPVOID lpBaseAddress,           // base of memory area
    //			LPCVOID lpBuffer,               // data buffer
    //			SIZE_T nSize,                   // count of bytes to write
    //			SIZE_T * lpNumberOfBytesWritten // count of bytes written
    //			);
    [DllImport("kernel32.dll")]
    public static extern Int32 WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [In, Out] byte[] buffer, UInt32 size, out IntPtr lpNumberOfBytesWritten);
}
