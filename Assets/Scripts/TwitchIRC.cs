using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.IO;
using UnityEngine.SceneManagement;

public class TwitchIRC : MonoBehaviour
{
    TcpClient twitchClient;
    StreamReader reader;
    StreamWriter writer;

    string username = "skylarrider", password = "oauth:b8rtevb9bp7xmt2my5fzznt86qg3b9", channelName = "skylarrider";

    [SerializeField] Transform trsCube;

    float rotSpeed = 0;

    [SerializeField] float moveSpeed;

    [SerializeField] bool isMoving;

    bool isGameEnded;

    [SerializeField] GameObject winText;

    Rigidbody rb;

    Vector3 respawnPoint;
    Quaternion respawnAxis;

    [SerializeField] string nextScene;

    void Start()
    {
        Connect();

        rb = GetComponent<Rigidbody>();
        respawnPoint = trsCube.position;
        respawnAxis = trsCube.rotation;
    }

    void Update()
    {
        if(!Connected)
        {
            Connect();
        }
        //trsCube.Rotate(Vector3.forward * rotSpeed * Time.deltaTime);
        ReadChat();

        if(isMoving)
        {
            trsCube.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
        }
    }

   void Connect()
   {
        twitchClient = new TcpClient("irc.chat.twitch.tv", 6667);
        writer = new StreamWriter(twitchClient.GetStream());
        reader = new StreamReader(twitchClient.GetStream());

        writer.WriteLine("PASS " + password);
        writer.WriteLine("NICK " + username);
        writer.WriteLine("USER " + username + " 8 * :" + username);
        writer.WriteLine("JOIN #" + channelName);
        writer.Flush();
   }

   void ReadChat()
   {
       if(HasMessage && !isGameEnded)
       {
           string message = reader.ReadLine();
           if(message.Contains("PRIVMSG"))
           {
                int splitPoint = message.IndexOf(":", 1);
                message = message.Substring(splitPoint + 1).ToLower();

                if(message.Equals("!move"))
                {
                    isMoving = true;
                }
                if(message.Equals("!stop"))
                {
                    isMoving = false;
                }

                if(message.Equals("!front") || message.Equals("!top"))
                {
                    trsCube.rotation = Quaternion.LookRotation(Vector3.forward);
                }
                if(message.Equals("!back") || message.Equals("!bot") || message.Equals("!bottom"))
                {
                    trsCube.rotation = Quaternion.LookRotation(Vector3.back);
                }
                if(message.Equals("!left"))
                {
                    trsCube.rotation = Quaternion.LookRotation(Vector3.left);
                }
                if(message.Equals("!right"))
                {
                    trsCube.rotation = Quaternion.LookRotation(Vector3.right);
                }
                
                if(message.Equals("!jump"))
                {
                    Debug.Log("You're jumping!");
                    rb.AddForce(transform.up * 500.0f);
                }
           }
       }
   }

    void OnTriggerEnter(Collider other) 
    {
        if(other.CompareTag("end"))
        {
            isMoving = false;
            isGameEnded = true;
            winText.SetActive(true);
            StartCoroutine(ChangeScene());
        }
        if(other.CompareTag("Respawn"))
        {
            isMoving = false;

            trsCube.position = respawnPoint;
            trsCube.rotation = respawnAxis;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    IEnumerator ChangeScene()
    {
        yield return new WaitForSeconds(1.1f);

        SceneManager.LoadScene(nextScene);
    }


   bool Connected => twitchClient.Connected;

   bool HasMessage => twitchClient.Available > 0;
}
