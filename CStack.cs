using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inlamning_3_ra_kod
{
    /* CLASS: CStack
     * PURPOSE: Is essentially a RPN-calculator with four registers X, Y, Z, T
     *   like the HP RPN calculators. Numeric values are entered in the entry
     *   string by adding digits and one comma. For test purposes the method
     *   RollSetX can be used instead. Operations can be performed on the
     *   calculator preferrably by using one of the methods
     *     1. BinOp - merges X and Y into X via an operation and rolls down
     *        the stack
     *     2. Unop - operates X and puts the result in X with overwrite
     *     3. Nilop - adds a known constant on the stack and rolls up the stack
     */
    public class CStack
    {
        public double X, Y, Z, T;
        public string entry, entryVar;
        public string[,] address;
        public string _path;
        public bool fileExist = false;
        /* CONSTRUCTOR: CStack
         * PURPOSE: create a new stack and init X, Y, Z, T,A,B,C,D,E,F,G,H
         * and the text entry for variabels: X, Y, Z, T and entryVar for variabels:A,B,C,D,E,F,G,H
         * PARAMETERS: --
         */
        public CStack()
        {
            X = Y = Z = T = 0;
            address = new string[8, 2] {
                { "A", "0" },
                { "B", "0" },
                { "C", "0" },
                { "D", "0" },
                { "E", "0" },
                { "F", "0" },
                { "G", "0" },
                { "H", "0" }
            };
            entry = "";
            entryVar = "";
        }
        /* CONSTRUCTOR: CStack
         * PURPOSE: Create a new stack and init X, Y, Z, T,A,B,C,D,E,F,G,H
         * and the text entry for variabels: X, Y, Z, T and entryVar for variabels:A,B,C,D,E,F,G,H
         * PARAMETERS: A File path that containt operations variabels value X, Y, Z, T,A,B,C,D,E,F,G,H
         */

        public CStack(string path, bool pathDefault = false)
        {
            _path = pathDefault ? @".\variabelFile.txt" : path;
            address = new string[8, 2] {
                { "A", "0" },
                { "B", "0" },
                { "C", "0" },
                { "D", "0" },
                { "E", "0" },
                { "F", "0" },
                { "G", "0" },
                { "H", "0" }
            };

            entry = "";
            entryVar = "";

            if (!File.Exists(_path))
                return;

            fileExist = true;
            using (StreamReader sr = new StreamReader(_path))
            {
                int i = 0;
                while (sr.Peek() >= 0)
                {
                    string line = sr.ReadLine();
                    string[] data = line.Split(',');

                    if(data[0] == "1")
                    {
                        address[i, 1] = data[2];
                    } else
                    {
                        if (data[1] == "X") { X = double.Parse(data[2]); }
                        if (data[1] == "Y") { Y = double.Parse(data[2]); }
                        if (data[1] == "T") { T = double.Parse(data[2]); }
                        if (data[1] == "Z") { Z = double.Parse(data[2]); }
                    }
                    i++;
                }
            }
        }

        /* METHOD: Exit
         * PURPOSE: called on exit, prepared for saving
         * PARAMETERS: --
         * RETURNS: --
         */
        public void Exit()
        {
            SaveVarInFile();
        }
        /* METHOD: StackString
         * PURPOSE: construct a string to write out in a stack view
         * PARAMETERS: --
         * RETURNS: the string containing the values T, Z, Y, X with newlines 
         *   between them
         */
        public string StackString()
        {
            return $"{T}\n{Z}\n{Y}\n{X}\n{entry}";
        }
        /* METHOD: VarString
         * PURPOSE: construct a string to write out in a variable list
         * PARAMETERS: --
         * RETURNS: --
         */
        public string VarString()
        {
            return $"{address[0, 1]}\n{address[1, 1]}\n{address[2, 1]}\n{address[3, 1]}\n{address[4, 1]}\n{address[5, 1]}\n{address[6, 1]}\n{address[7, 1]}";
        }
        /* METHOD: SetX
         * PURPOSE: set X with overwrite
         * PARAMETERS: double newX - the new value to put in X
         * RETURNS: --
         */
        public void SetX(double newX)
        {
            X = newX;
        }
        /* METHOD: EntryAddNum
         * PURPOSE: add a digit to the entry string
         * PARAMETERS: string digit - the candidate digit to add at the end of the
         *   string
         * RETURNS: --
         * FAILS: if the string digit does not contain a parseable integer, nothing
         *   is added to the entry
         *  FAILS SOLVED: it compare every digit introduce, if is digit or not.
         */
        public void EntryAddNum(string digit)
        {
            int val;
            bool isDigit = true;

            for (int i = 0; i < digit.Length; i++)
            {
                if (!Char.IsDigit(digit[i]))
                {
                    isDigit = false;
                }
            }

            if (isDigit && int.TryParse(digit, out val))
            {
                entry = entry + val;
            }
        }
        /* METHOD: EntryAddComma
         * PURPOSE: adds a comma to the entry string
         * PARAMETERS: --
         * RETURNS: --
        
         */
        public void EntryAddComma()
        {
            if (entry.IndexOf(",") == -1)
                entry = entry + ",";
        }
        /* METHOD: EntryChangeSign
         * PURPOSE: changes the sign of the entry string
         * PARAMETERS: --
         * RETURNS: --
         * FEATURES: if the first char is already a '-' it is exchanged for a '+',
         *   if it is a '+' it is changed to a '-', otherwise a '-' is just added
         *   first
         */
        public void EntryChangeSign()
        {
            char[] cval = entry.ToCharArray();
            if (cval.Length > 0)
            {
                switch (cval[0])
                {
                    case '+': cval[0] = '-'; entry = new string(cval); break;
                    case '-': cval[0] = '+'; entry = new string(cval); break;
                    default: entry = '-' + entry; break;
                }
            }
            else
            {
                entry = '-' + entry;
            }
        }
        /* METHOD: Enter
         * PURPOSE: converts the entry to a double and puts it into X
         * PARAMETERS: --
         * RETURNS: --
         * FEATURES: the entry is cleared after a successful operation
         */
        public void Enter()
        {
            if (entry != "")
            {
                RollSetX(double.Parse(entry));
                entry = "";
            }
        }
        /* METHOD: Drop
         * PURPOSE: drops the value of X, and rolls down
         * PARAMETERS: --
         * RETURNS: --
         * FEATURES: Z gets the value of T
         */
        public void Drop()
        {
            X = Y; Y = Z; Z = T; T = 0;
        }
        /* METHOD: DropSetX
         * PURPOSE: replaces the value of X, and rolls down
         * PARAMETERS: double newX - the new value to assign to X
         * RETURNS: --
         * FEATURES: Z gets the value of T
         * NOTES: this is used when applying binary operations consuming
         *   X and Y and putting the result in X, while rolling down the
         *   stack
         */
        public void DropSetX(double newX)
        {
            X = newX; Y = Z; Z = T;
        }
        /* METHOD: BinOp
         * PURPOSE: evaluates a binary operation
         * PARAMETERS: string op - the binary operation retrieved from the
         *   GUI buttons
         * RETURNS: --
         * FEATURES: the stack is rolled down
         */
        public void BinOp(string op)
        {
            switch (op)
            {
                case "+": DropSetX(Y + X); break;
                case "−": DropSetX(Y - X); break;
                case "×": DropSetX(Y * X); break;
                case "÷": DropSetX(Y / X); break;
                case "yˣ": DropSetX(Math.Pow(Y, X)); break;
                case "ˣ√y": DropSetX(Math.Pow(Y, 1.0 / X)); break;
            }
        }
        /* METHOD: Unop
         * PURPOSE: evaluates a unary operation
         * PARAMETERS: string op - the unary operation retrieved from the
         *   GUI buttons
         * RETURNS: --
         * FEATURES: the stack is not moved, X is replaced by the result of
         *   the operation
         */
        public void Unop(string op)
        {
            switch (op)
            {
                // Powers & Logarithms:
                case "x²": SetX(X * X); break;
                case "√x": SetX(Math.Sqrt(X)); break;
                case "log x": SetX(Math.Log10(X)); break;
                case "ln x": SetX(Math.Log(X)); break;
                case "10ˣ": SetX(Math.Pow(10, X)); break;
                case "eˣ": SetX(Math.Exp(X)); break;

                // Trigonometry:
                case "sin": SetX(Math.Sin(X)); break;
                case "cos": SetX(Math.Cos(X)); break;
                case "tan": SetX(Math.Tan(X)); break;
                case "sin⁻¹": SetX(Math.Asin(X)); break;
                case "cos⁻¹": SetX(Math.Acos(X)); break;
                case "tan⁻¹": SetX(Math.Atan(X)); break;
            }
        }
        /* METHOD: Nilop
         * PURPOSE: evaluates a "nilary operation" (insertion of a constant)
         * PARAMETERS: string op - the nilary operation (name of the constant)
         *   retrieved from the GUI buttons
         * RETURNS: --
         * FEATURES: the stack is rolled up, X is preserved in Y that is preserved in
         *   Z that is preserved in T, T is erased
         */
        public void Nilop(string op)
        {
            switch (op)
            {
                case "π": RollSetX(Math.PI); break;
                case "e": RollSetX(Math.E); break;
            }
        }
        /* METHOD: Roll
         * PURPOSE: rolls the stack up
         * PARAMETERS: --
         * RETURNS: --
         */
        public void Roll()
        {
            double tmp = T;
            T = Z; Z = Y; Y = X; X = tmp;
        }
        /* METHOD: RollSetX
         * PURPOSE: rolls the stack up and puts a new value in X
         * PARAMETERS: double newX - the new value to put into X
         * RETURNS: --
         * FEATURES: T is dropped
         */
        public void RollSetX(double newX)
        {
            T = Z; 
            Z = Y; 
            Y = X; 
            X = newX;
        }
        /* METHOD: SetAddress
         * PURPOSE: 
         * PARAMETERS: string name - variable name
         * RETURNS: --
         * FEATURES: NOT YET IMPLEMENTED
         */
        public void SetAddress(string name)
        {
            entryVar = name;

        }
        /* METHOD: SetVar
         * PURPOSE: Assign a value to a specific variabel from A ..H
         * PARAMETERS: --
         * RETURNS: --
         */
        public void SetVar()
        {
            for (int i = 0; i < address.GetLength(0); i++)
            {
                if (address[i, 0] == entryVar)
                {
                    address[i, 1] = X.ToString();
                }
            }
        }
        /* METHOD: GetVar
         * PURPOSE: Get the value of a specific variabel from A ..H
         * PARAMETERS: --
         * RETURNS: --
         */
        public void GetVar()
        {
            for (int i = 0; i < address.GetLength(0); i++)
            {
                if (address[i, 0] == entryVar)
                {
                    entry = address[i, 1];
                }
            }

            X = Y; Y = Z; Z = T; T = 0;
        }
        /* METHOD: SaveVarInFile
       * PURPOSE: Save the valueS of all variabels from A ..H and X,Y,Z,T in a File .txt
       * PARAMETERS: --
       * RETURNS: --
       */
        public void SaveVarInFile()
        {
            if (!fileExist)
                return;

            string pathCopy = _path.Substring(0, _path.Length - 4) + "_copy" + ".txt";
            using (StreamWriter sw = File.CreateText(pathCopy))
            {
                for (int i = 0; i < address.GetLength(0); i++)
                {
                    string line = "1," + address[i, 0] + "," + address[i, 1];
                    sw.WriteLine(line);
                }
                string lineT = "2,T," + T;
                sw.WriteLine(lineT);
                string lineZ = "2,Z," + Z;
                sw.WriteLine(lineZ);
                string lineY = "2,Y," + Y;
                sw.WriteLine(lineY);
                string lineX = "2,X," + X;
                sw.WriteLine(lineX);
            }
            File.Delete(_path);
            File.Move(pathCopy, _path);
        }
    }
}

