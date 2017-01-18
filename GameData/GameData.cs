﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
namespace GameData
{

   public class LeavingData
    {
        public string playerID;
        public string Tag;
    }

    public class ScoreData
    {
        public string playerID;
        public string Tag;
        public int score;
    }

    public enum GameState { WAITING,STARTING,STARTED,FINISHED};

    public class TimerData
    {
        public GameState gamestate;
        public int Seconds;
    }


    public class SetWorldSize
    {
        public int X;
        public int Y;

    }

    public enum COLLECTABLE_ACTION { DELIVERED, COLLECTED, DELETED}

    public class CollectableData
    {
        public COLLECTABLE_ACTION ACTION;
        public string collectableId;
        public string CollectableName;
        public int collectableValue;        
        public int X;
        public int Y;
    }

    public class Joined
    {
        public string playerID;
        public float X;
        public float Y;
        public string imageName;
    }

    public class JoinRequestMessage
    {
        public string TagName;
        public string Password;
    }

    public class TestMess
    {
        public string message = "Hello there";
    }

    public class MoveMessage
    {
        public string playerID;
        public float NewX;    
        public float NewY;
    }

    public class ErrorMess
    {
        public string message = "Error --> ";
    }

    public class GameMessage
    {
        public string message = "Error --> ";
    }

    public class GamePacket<T>{
        public GamePacket(T obj)
        {
            val = obj;
        }
        public T val;
        public Type type;
    }



}
