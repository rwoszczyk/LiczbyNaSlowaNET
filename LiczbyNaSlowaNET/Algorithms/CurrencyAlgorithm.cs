﻿
// Copyright (c) 2014 Przemek Walkowski

namespace LiczbyNaSlowaNET.Algorithms
{
    using System.Linq;
    using System.Text;

    using Dictionaries.Currencies;
    using Dictionaries;

    internal sealed class CurrencyAlgorithm : Algorithm
    {
        public CurrencyAlgorithm(IDictionaries dictionary) :
           base(dictionary)
        { }

        private readonly StringBuilder result = new StringBuilder();
       
        private int hundreds;
                
        private int tens;

        private int unity;

        private int othersTens;

        private int order;

        private Phase currentPhase;

        private readonly int[] tempGrammarForm = { 2, 3, 4 };

        public override string Build()
        {
            this.currentPhase = Phase.BeforeComma;

            foreach (var number in Numbers)
            {
                var partialResult = new StringBuilder();

                if (number == 0)
                {
                    partialResult.Append(Dictionaries.Unity[10]);

                    partialResult.Append(" ");

                    partialResult.Append(this.Options.Currency.GetDeflationTable[(int) currentPhase, 2]);

                    result.Append(partialResult.ToString().Trim());

                    result.Append(" ");

                    this.currentPhase = Phase.AfterComma;

                    continue;
                   
                }

                if (number < 0)
                {
                    partialResult.Append(Dictionaries.Sign[2]);
                }

                var tempNumber = number;

                this.order = 0;

                while (tempNumber != 0)
                {
                    this.hundreds = (tempNumber % 1000) / 100;

                    this.tens = (tempNumber % 100) / 10;

                    this.unity = tempNumber % 10;

                    if (this.tens == 1 && this.unity > 0)
                    {
                        this.othersTens = this.unity;
                        this.tens = 0;
                        this.unity = 0;
                    }
                    else
                    {
                        this.othersTens = 0;
                    }

                    var grammarForm = this.GetGrammaForm();
                    

                    if ((this.hundreds + this.unity + this.othersTens + this.tens) > 0)
                    {
                        var tempPartialResult = partialResult.ToString().Trim();

                        partialResult.Clear();
                        var properUnity = Dictionaries.Unity;

                        if (currentPhase == Phase.AfterComma && this.Options.Currency is ICurrencyNotMaleDeflectionAfterComma && this.tens == 0)
                        {
                            properUnity = (this.Options.Currency as ICurrencyNotMaleDeflectionAfterComma).OverrideAfterCommaUnity;
                        }

                        if (currentPhase == Phase.BeforeComma && this.Options.Currency is ICurrencyNotMaleDeflectionBeforeComma)
                        {
                            properUnity = (this.Options.Currency as ICurrencyNotMaleDeflectionBeforeComma).OverrideBeforeCommaUnity;
                        }

                        partialResult.AppendFormat("{0}{1}{2}{3}{4}{5}",
                            this.SetSpaceBeforeString(Dictionaries.Hundreds[this.hundreds]),
                            this.SetSpaceBeforeString(Dictionaries.Tens[this.tens]),
                            this.SetSpaceBeforeString(Dictionaries.OthersTens[this.othersTens]),
                            this.SetSpaceBeforeString(properUnity[this.unity]),
                            this.SetSpaceBeforeString(Dictionaries.Endings[this.order, grammarForm]),
                            this.SetSpaceBeforeString(tempPartialResult));
                    }

                    this.order += 1;

                    tempNumber = tempNumber / 1000;
                }

                partialResult.Append(this.SetSpaceBeforeString(this.Options.Currency.GetDeflationTable[(int)this.currentPhase, GetCurrencyForm(number)]));

                result.Append(partialResult.ToString().Trim());

                result.Append(" ");

                if (this.currentPhase == Phase.BeforeComma && !string.IsNullOrEmpty(Options.SplitDecimal))
                {
                    result.Append(Options.SplitDecimal);
                    result.Append(" ");
                }

                this.currentPhase = Phase.AfterComma;
            }

            return result.ToString().Trim();
        }

        private int GetCurrencyForm(int number)
        {
            var hundreds = (number % 1000) / 100;

            var tens = (number % 100) / 10;

            var unity = number % 10;

            if (unity == 1 && (hundreds + tens + othersTens == 0))
            {
                return 0;
            }

            if (tempGrammarForm.Contains(unity) && tens != 1)
            {
                return 1;
            }

            return 2;
        }

        private int GetGrammaForm()
        {
            if (this.unity == 1 && (this.hundreds + this.tens + this.othersTens == 0))
            {
                return  0;
            }

            if (tempGrammarForm.Contains(this.unity))
            {
                return 1;
            }

            return 2;
        }
    }
}
