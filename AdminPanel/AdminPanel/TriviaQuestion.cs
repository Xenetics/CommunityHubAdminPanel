using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace AdminPanel
{
    // Hold the information on a question in the database
    public class TriviaQuestion : TableEntity
    {
        // Organization the question is assosiated with
        public string Org { get; set; }
        // Location for the question
        public string Location { get; set; }
        // Question Text
        public string Question { get; set; }
        // Answer option A
        public string AnswerA { get; set; }
        // Answer option B
        public string AnswerB { get; set; }
        // Answer option C
        public string AnswerC { get; set; }
        // Answer option D
        public string AnswerD { get; set; }
        // Correct Answer
        public string CorrectAnswer { get; set; }
        // Token Value
        public int Value { get; set; }

        //Default constructor
        public TriviaQuestion() { }
        // Constructor
        public TriviaQuestion(string partitionKey, string org, string location, string question, string answerA, string answerB, string answerC, string answerD, string correctAnswer, int value)
        {
            this.PartitionKey = partitionKey;
            this.RowKey = org;
            Org = org;
            Location = location;
            Question = question;
            AnswerA = answerA;
            AnswerB = answerB;
            AnswerC = answerC;
            AnswerD = answerD;
            CorrectAnswer = correctAnswer;
            Value = value;
        }

        // Returns a string for display in a list
        public override string ToString()
        {
            return String.Format("{0, -32} | {1, -32} | {2, -12} | {3, -16} | {4, -12} | {5, -12} | {6, -12} | {7, -12} | {8, -6} \t", new string[] { Org, Location, Question, CorrectAnswer, AnswerA, AnswerB, AnswerC, AnswerD, Value.ToString() });
        }
    }
}
