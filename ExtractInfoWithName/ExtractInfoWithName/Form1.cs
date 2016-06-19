using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


using CSALMongo;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ExtractInfoWithName
{
    public partial class Form1 : Form
    {

        //should change the name of the database
        public const string DB_URL = "mongodb://localhost:27017/csaldata";

        public Form1()
        {
            InitializeComponent();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            var allnames = richTextBox1.Text.ToString();

            var names = allnames.Split('\n');
            string lessonID = "lesson4";

            foreach (var name in names)
            {
                String nameId = name.Split(new String[] { " " }, StringSplitOptions.None)[1];
                string studentId = nameId;
                string allRecord = "";

                String perRecord = getPerRecord4(studentId, lessonID);
                if (perRecord == null)
                {
                    continue;
                }
                else
                {
                    String[] record1 = perRecord.Split(new String[] { "\n" }, StringSplitOptions.None);
                    foreach (String record in record1)
                    {
                        allRecord += name + "\t" + "Megan" + "\t" + record + "\n";
                    }
                }

                this.richTextBox1.AppendText(allRecord);
            }


        }

        // fill record with different question number
        public String fillRecord(List<String> questionScore, List<String> questionDura, int maxQuestionNum)
        {
            String halfRecord = "";
            int count = 0;
            if (questionScore == null || questionScore.Count < 1)
            {
                for (int i = 0; i < maxQuestionNum; i++)
                {
                    halfRecord += "\t";
                    count++;
                }
            }
            else
            {
                foreach (String quesScore in questionScore)
                {
                    halfRecord += quesScore + "\t";
                    count++;
                }
            }

            for (int i = 0; i < maxQuestionNum - count; i++)
            {
                halfRecord += "\t";
            }

            if (questionDura == null || questionDura.Count < 1)
            {
                for (int i = 0; i < maxQuestionNum; i++)
                {
                    halfRecord += "\t";
                    count++;
                }
            }
            else
            {
                foreach (String quesDura in questionDura)
                {
                    halfRecord += quesDura + "\t";
                }
            }

            for (int i = 0; i < maxQuestionNum - count; i++)
            {
                halfRecord += "\t";
            }

            return halfRecord;
        }

        // lesson 4, 
        public String getPerRecord4(String studentName, String lessonID)
        {
            String sectionLevel = "Medium", oneRecord = "", questionRow = "";
            var db = new CSALDatabase(DB_URL);
            var oneTurn = db.FindTurns(lessonID, studentName);
            int lastTurnID = 99, attempCount = 0, thisAttempCount = 0, sectionFlag = 0;
            double score = 0, duration = 0;
            bool getAnswer = false;

            List<double> attempTime = new List<double>();

            // medium
            List<String> questionScore = new List<string>();
            List<String> questionDura = new List<string>();

            // easy
            List<String> questionEasyScore = new List<string>();
            List<String> questionEasyDura = new List<string>();

            // hard
            List<String> questionHardScore = new List<string>();
            List<String> questionHardDura = new List<string>();

            // Medium 2
            List<String> questionM2Score = new List<string>();
            List<String> questionM2Dura = new List<string>();

            if (oneTurn == null || oneTurn.Count < 1 || oneTurn[0].Turns.Count < 1)
            {
                return null;
            }
            else
            {
                // calculate total time of every Attempt
                foreach (var turn in oneTurn[0].Turns)
                {
                    if (turn.TurnID < lastTurnID)
                    {
                        attempCount++;
                        double turnDura = (int)turn.Duration;
                        turnDura = turnDura / 1000;
                        attempTime.Add(turnDura);
                    }
                    else
                    {
                        double turnDura = (int)turn.Duration;
                        attempTime[attempCount - 1] += turnDura / 1000;
                    }
                    lastTurnID = turn.TurnID;
                }

                lastTurnID = 0;
                foreach (var turn in oneTurn[0].Turns)
                {
                    // student tried more than 1, reset everything
                    if (turn.TurnID < lastTurnID)
                    {
                        oneRecord += (thisAttempCount + 1).ToString() + "\t" + attempTime[thisAttempCount].ToString() + "\t" +
                                fillRecord(questionScore, questionDura, 5) + fillRecord(questionEasyScore, questionEasyDura, 10) +
                                fillRecord(questionHardScore, questionHardDura, 10) + fillRecord(questionM2Score, questionM2Dura, 17);
                        thisAttempCount++;
                        score = 0;
                        duration = 0;
                        lastTurnID = 0;
                        sectionFlag = 0;
                        questionScore.Clear();
                        questionDura.Clear();
                        questionEasyScore.Clear();
                        questionEasyDura.Clear();
                        questionHardScore.Clear();
                        questionHardDura.Clear();
                        questionM2Score.Clear();
                        questionM2Dura.Clear();
                    }
                    else
                    {
                        duration = (int)turn.Duration;
                        duration = duration / 1000;

                        // correctness
                        if (turn.Input.Event == "Correct" && sectionFlag == 0)
                        {
                            score = 1;
                            questionScore.Add(score.ToString());
                            questionDura.Add(duration.ToString());
                        }
                        else if (turn.Input.Event == "Incorrect" && sectionFlag == 0)
                        {
                            score = 0;
                            questionScore.Add(score.ToString());
                            questionDura.Add(duration.ToString());
                        }
                        else if (turn.Input.Event == "Correct" && sectionFlag == 1)
                        {
                            score = 1;
                            questionEasyScore.Add(score.ToString());
                            questionEasyDura.Add(duration.ToString());
                        }
                        else if ((turn.Input.Event == "Incorrect") && sectionFlag == 1)
                        {
                            score = 0;
                            questionEasyScore.Add(score.ToString());
                            questionEasyDura.Add(duration.ToString());
                        }
                        else if (turn.Input.Event == "Correct" && sectionFlag == 2)
                        {
                            score = 1;
                            questionHardScore.Add(score.ToString());
                            questionHardDura.Add(duration.ToString());
                        }
                        else if ((turn.Input.Event == "Incorrect") && sectionFlag == 2)
                        {
                            score = 0;
                            questionHardScore.Add(score.ToString());
                            questionHardDura.Add(duration.ToString());
                        }
                        else if (turn.Input.Event == "Correct" && sectionFlag == 3)
                        {
                            score = 1;
                            questionM2Score.Add(score.ToString());
                            questionM2Dura.Add(duration.ToString());
                        }
                        else if ((turn.Input.Event == "Incorrect") && sectionFlag == 3)
                        {
                            score = 0;
                            questionM2Score.Add(score.ToString());
                            questionM2Dura.Add(duration.ToString());
                        }

                        foreach (var transition in turn.Transitions)
                        {
                            // section level
                            if (transition.RuleID == "GetTutoringPackEasy" && sectionFlag == 0)
                            {
                                sectionFlag = 1;
                            }
                            if (transition.RuleID == "GetTutoringPackHard" && sectionFlag == 0)
                            {
                                sectionFlag = 2;
                            }
                            if (turn.Input.Event.ToString().Contains("Level2_Diagnostic"))
                            {
                                sectionFlag = 3;
                            }
                        }
                    }

                    lastTurnID = turn.TurnID;
                }
                oneRecord += (thisAttempCount + 1).ToString() + "\t" + attempTime[thisAttempCount].ToString() + "\t" +
                        fillRecord(questionScore, questionDura, 5) + fillRecord(questionEasyScore, questionEasyDura, 10) +
                        fillRecord(questionHardScore, questionHardDura, 10) + fillRecord(questionM2Score, questionM2Dura, 15);
            }
            oneRecord += oneRecord;

            return oneRecord;
        }

        // lesson 18, 
        public String getPerRecord18(String studentName, String lessonID)
        {
            String sectionLevel = "Medium", oneRecord = "";
            var db = new CSALDatabase(DB_URL);
            var oneTurn = db.FindTurns(lessonID, studentName);
            int lastTurnID = 99, attempCount = 0, thisAttempCount = 0, questionIndex = 0, attempt = 0;
            Boolean reachAskQ = false;
            double score = 0, duration = 0, secondDura = 0;

            List<double> attempTime = new List<double>();

            // medium
            List<String> questionScore = new List<string>();
            List<String> questionDura = new List<string>();

            // medium second trial
            List<String> questionSecondScore = new List<string>();
            List<String> questionSecondDura = new List<string>();

            // initial all the list with 16 items
            for (int i = 0; i < 16; i++)
            {
                questionScore.Add(' '.ToString());
                questionDura.Add(' '.ToString());
                questionSecondScore.Add(' '.ToString());
                questionSecondDura.Add(' '.ToString());
            }

            if (oneTurn == null || oneTurn.Count < 1 || oneTurn[0].Turns.Count < 1)
            {
                return null;
            }
            else
            {
                // calculate total time of every Attempt
                foreach (var turn in oneTurn[0].Turns)
                {
                    if (turn.TurnID < lastTurnID)
                    {
                        attempCount++;
                        double turnDura = (int)turn.Duration;
                        turnDura = turnDura / 1000;
                        attempTime.Add(turnDura);
                    }
                    else
                    {
                        double turnDura = (int)turn.Duration;
                        attempTime[attempCount - 1] += turnDura / 1000;
                    }
                    lastTurnID = turn.TurnID;
                }

                lastTurnID = 0;
                foreach (var turn in oneTurn[0].Turns)
                {
                    // student tried more than 1, reset everything
                    if (turn.TurnID < lastTurnID)
                    {
                        oneRecord += (thisAttempCount + 1).ToString() + "\t" + attempTime[thisAttempCount].ToString() + "\t"
                            + fillRecord(questionScore, questionDura, 16) + fillRecord(questionSecondScore, questionSecondDura, 16);
                        thisAttempCount++;
                        score = 0;
                        duration = 0;
                        lastTurnID = 0;

                        questionScore.Clear();
                        questionDura.Clear();
                        questionSecondScore.Clear();
                        questionSecondDura.Clear();

                        for (int i = 0; i < 16; i++)
                        {
                            questionScore.Add(' '.ToString());
                            questionDura.Add(' '.ToString());
                            questionSecondScore.Add(' '.ToString());
                            questionSecondDura.Add(' '.ToString());
                        }

                    }
                    else
                    {
                        duration = (int)turn.Duration;
                        duration = duration / 1000;

                        foreach (var transition in turn.Transitions)
                        {
                            if (transition.RuleID.Contains("AskQ"))
                            {
                                reachAskQ = true;
                                int index = transition.RuleID.IndexOf("AskQ");
                                string cleanQues = (index < 0)
                                    ? transition.RuleID
                                    : transition.RuleID.Remove(index, "AskQ".Length);

                                questionIndex = Int32.Parse(cleanQues.Split(new Char[] { '.' })[0]);
                                attempt = Int32.Parse(cleanQues.Split(new Char[] { '.' })[1]);
                                break;
                            }
                        }
                    }

                    if (turn.Input.Event.Contains("Correct") && reachAskQ == true)
                    {
                        reachAskQ = false;
                        if (attempt == 1)
                        {
                            questionScore[questionIndex - 1] = '1'.ToString();
                            questionDura[questionIndex - 1] = duration.ToString();
                        }
                        else if (attempt == 2)
                        {
                            questionSecondScore[questionIndex - 1] = "0.5".ToString();
                            questionSecondDura[questionIndex - 1] = duration.ToString();
                        }
                    }
                    else if (turn.Input.Event.Contains("Incorrect") && reachAskQ == true)
                    {
                        reachAskQ = false;
                        if (attempt == 1)
                        {
                            questionScore[questionIndex - 1] = '0'.ToString();
                            questionDura[questionIndex - 1] = duration.ToString();
                        }
                        else if (attempt == 2)
                        {
                            questionSecondScore[questionIndex - 1] = "0".ToString();
                            questionSecondDura[questionIndex - 1] = duration.ToString();
                        }
                    }

                    lastTurnID = turn.TurnID;
                }
                oneRecord += (thisAttempCount + 1).ToString() + "\t" + attempTime[thisAttempCount].ToString() + "\t"
                             + fillRecord(questionScore, questionDura, 16) + fillRecord(questionSecondScore, questionSecondDura, 16);
            }
            return oneRecord;
        }
    }
}
