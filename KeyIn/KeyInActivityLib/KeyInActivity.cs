using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using System.ComponentModel;
using System.Threading;
using System.Collections;
using System.Windows.Forms;
using System.Activities.Presentation.PropertyEditing;
using System.Runtime.InteropServices;

namespace ktds.Ant.Activities
{

    [Designer(typeof(KeyInActivityDesigner))]
    public sealed class KeyInActivity : CodeActivity
    {
        private string mKeyInText = "";
        private int mDelayMiliSec = 1;

        static private Queue m_qKey = new Queue();

        // 형식 문자열의 작업 입력 인수를 정의합니다.
        public InArgument<string> Text { get; set; }

        [Category("KeyIn Info")]
        [Editor(typeof(TextBox), typeof(ExtendedPropertyValueEditor))]
        public string KeyInText { get; set; }

        [Category("KeyIn Info")]
        [Editor(typeof(TextBox), typeof(ExtendedPropertyValueEditor))]
        public int DelayMiliSec { get; set; }

        [Category("Output")]
        public OutArgument<bool> ResultBool { get; set; }

        [DllImport("User32.dll")]
        public static extern void keybd_event(int vk, uint scan, uint flags, uint extraInfo);
        public const int KEYEVENTF_KEYDOWN = 0x00;
        public const int KEYEVENTF_EXTENDEDKEY = 0x1;
        public const int KEYEVENTF_KEYUP = 0x02;


        [DllImport("user32.dll")]
        private static extern int GetKeyState(int nVirtKey);
        public const int VK_HANGUL = 0x15;

        Dictionary<string, int> SpecialKeyDic = new Dictionary<string, int>()
        {
            {"BACKSPACE",  (int) Keys.Back },
            {"TAB",        (int) Keys.Tab },
            {"ENTER",      (int) Keys.Enter },
            {"SHIFT",      (int) Keys.ShiftKey },
            {"CTRL",       (int) Keys.ControlKey },
            {"ALT",        (int) Keys.Menu },
            {"PAUSEBREAK", (int) Keys.Pause },
            {"CAPSLOOK",   (int) Keys.CapsLock },
            {"KOR_ENG",    (int) Keys.HangulMode },
            {"HANJA",      (int) Keys.HanjaMode },
            {"ESC",        (int) Keys.Escape },
            {"SPACE",      (int) Keys.Space },
            {"PAGEUP",     (int) Keys.PageUp },
            {"PAGEDN",     (int) Keys.PageDown },
            {"END",        (int) Keys.End },
            {"HOME",       (int) Keys.Home },
            {"LEFT",       (int) Keys.Left },
            {"UP",         (int) Keys.Up },
            {"RIGHT",      (int) Keys.Right },
            {"DOWN",       (int) Keys.Down },
            {"INS",        (int) Keys.Insert },
            {"DEL",        (int) Keys.Delete },
            {"WIN",        (int) Keys.LWin },
            {"FN",         (int) Keys.Apps },
            {"F1",         (int) Keys.F1 },
            {"F2",         (int) Keys.F2 },
            {"F3",         (int) Keys.F3 },
            {"F4",         (int) Keys.F4 },
            {"F5",         (int) Keys.F5 },
            {"F6",         (int) Keys.F6 },
            {"F7",         (int) Keys.F7 },
            {"F8",         (int) Keys.F8 },
            {"F9",         (int) Keys.F9 },
            {"F10",        (int) Keys.F10 },
            {"F11",        (int) Keys.F11 },
            {"F12",        (int) Keys.F12 },
            {"NUMLOCK",    (int) Keys.NumLock },
            {"SCROLLLOCK", (int) Keys.Scroll }
        };

        // 작업 결과 값을 반환할 경우 CodeActivity<TResult>에서 파생되고
        // Execute 메서드에서 값을 반환합니다.
        protected override void Execute(CodeActivityContext context)
        {
            // 텍스트 입력 인수의 런타임 값을 가져옵니다.
            string text = context.GetValue(this.Text);

            byte[] sText = Encoding.ASCII.GetBytes(KeyInText);
            //MessageBox.Show(KeyInText);

            m_qKey.Clear();

            if(text != "")
               KeyboardInput(KeyInText, mDelayMiliSec);

            //AddString(KeyInText, mDelayMiliSec);

            this.ResultBool.Set(context, true);

        }


        private void KeyboardInput(string sText, int mDelayMiliSec)
        {

            bool bSpclKey1 = false;
            bool bSpclKey2 = false;
            bool bSpclKey3 = false;
            bool bSpclKeyStart = false;
            bool bSpclKeyEnd = false;

            char[] array = sText.ToCharArray();
            List<string> WordList = new List<string>();
            string str = "";

            for (int i = 0; i < array.Length; i++)
            {
                char letter = array[i];

                str = str + letter.ToString();

                if (letter == '[' || letter == '^' || letter == ']')
                {

                    if (letter == '[')
                    {
                        bSpclKey1 = true;
                    }

                    if (letter == '^')
                    {
                        bSpclKey2 = true;

                        if (bSpclKey1)
                        {
                            bSpclKeyStart = true;
                            bSpclKeyEnd = false;
                            if (str.Length > 2)
                            {
                                WordList.Add(str.Substring(0, str.Length - 2));
                                str = "[^";
                            }
                        }
                    }

                    if (letter == ']')
                    {
                        bSpclKey3 = true;
                        if (bSpclKey2)
                        {
                            bSpclKeyStart = false;
                            bSpclKeyEnd = true;
                            WordList.Add(str);
                            str = "";
                        }
                    }
                }
                else
                {
                    bSpclKey1 = false;
                    bSpclKey2 = false;
                    bSpclKey3 = false;
                }

            }

            if (str != "")
                WordList.Add(str);


            Queue<int> KeyQueue = new Queue<int>();
            Stack<int> keyStack = new Stack<int>();


            List<string> SpclKeyList = new List<string>();

            str = "";
            foreach (string word in WordList)
            {
                //MessageBox.Show(word);
                string sKeys = word;
                SpclKeyList.Clear();

                if (word.Contains("[^") && word.Contains("^]"))
                {
                    sKeys = sKeys.Replace("[^", "");
                    sKeys = sKeys.Replace("^]", "");

                    array = sKeys.ToCharArray();

                    char prevLetter = ' ';
                    str = "";
                    for (int i = 0; i < array.Length; i++)
                    {
                        char letter = array[i];
                        str = str + letter.ToString();
                        if (letter == '+')
                        {
                            if (str.Length > 1)
                                SpclKeyList.Add(str.Substring(0, str.Length - 1));

                            if (prevLetter == '+')
                            {
                                SpclKeyList.Add("+");
                            }

                            str = "";
                        }

                        prevLetter = letter;
                    }

                    if (str != "")
                        SpclKeyList.Add(str);

                    int nKey;
                    foreach (string xx in SpclKeyList)
                    {
                        if (SpecialKeyDic.TryGetValue(xx.ToUpper(), out nKey))
                        {
                            keybd_event(nKey, 0x00, KEYEVENTF_KEYDOWN, 0);
                            keyStack.Push(nKey);
                            Thread.Sleep(mDelayMiliSec);
                        }
                        else
                        {
                            AddString(xx, mDelayMiliSec);
                        }
                    }



                    while (keyStack.Count() > 0)
                    {
                        nKey = keyStack.Pop();
                        keybd_event(nKey, 0x00, KEYEVENTF_KEYUP, 0);
                        Thread.Sleep(mDelayMiliSec);
                    }
                }
                else
                {
                    AddString(word, mDelayMiliSec);
                }
            }
        }


        private void AddString(String str, int nDelay)
        {
            if ((str != null) & (str != String.Empty))
            {
                foreach (Char ch in str)
                {
                    AddChar(ch, nDelay);
                }
            }
        }

        private void AddChar(Char chChar, int nDelay)
        {
            if (m_qKey != null)
            {
                KeyValuePair<Char, int> kv = new KeyValuePair<Char, int>(chChar, nDelay);

                PressKey(kv.Key, kv.Value);
                /*
                lock (m_qKey)
                {
                    m_qKey.Enqueue(kv);

                    
                }
                */
            }
        }

        public static void PressKey(Char chChar, int nDelay)
        {
            try
            {
                String[] astrSplit = SplitHangle(chChar, true);

                if (astrSplit != null)
                {
                    bool bHangle = astrSplit.Length == 3;

                    // 한글 설정
                    if (bHangle)
                    {
                        if (GetKeyState(VK_HANGUL) != 1)
                        { 
                            int nHangleKey = (int)Keys.HangulMode; // (int)Keys.HangulMode;  // (int)Keys.ProcessKey
                            keybd_event(nHangleKey, 0x00, KEYEVENTF_KEYDOWN, 0);
                            Thread.Sleep(nDelay);

                            keybd_event(nHangleKey, 0x00, KEYEVENTF_KEYUP, 0);
                            Thread.Sleep(nDelay);
                        }
                    }

                    foreach (String strSplit in astrSplit)
                    {
                        if (strSplit != String.Empty)
                        {
                            foreach (Char chSplit in strSplit)
                            {
                                bool bUpper = ('A' <= chSplit) & (chSplit <= 'Z');
                                bool bLower = ('a' <= chSplit) & (chSplit <= 'z');
                                int nValue;
                                bool bShift;

                                if (bUpper | bLower)
                                {
                                    if (bHangle)
                                    {
                                        bShift = false;
                                    }
                                    else
                                    {
                                        bool bCapsLock = Control.IsKeyLocked(Keys.CapsLock);    // true인 경우 대문자 입력 상태
                                        bShift = bCapsLock != bUpper;
                                    }

                                    if (bUpper)
                                    {
                                        nValue = (int)chSplit;
                                    }
                                    else
                                    {
                                        nValue = (int)(chSplit.ToString().ToUpper()[0]);
                                    }
                                }
                                else
                                {
                                    switch (chSplit)
                                    {
                                        case '~': bShift = true; nValue = (int)Keys.Oemtilde; break;
                                        case '_': bShift = true; nValue = (int)Keys.OemMinus; break;
                                        case '+': bShift = true; nValue = (int)Keys.Oemplus; break;
                                        case '{': bShift = true; nValue = (int)Keys.OemOpenBrackets; break;
                                        case '}': bShift = true; nValue = (int)Keys.OemCloseBrackets; break;
                                        case '|': bShift = true; nValue = (int)Keys.OemPipe; break;
                                        case ':': bShift = true; nValue = (int)Keys.OemSemicolon; break;
                                        case '"': bShift = true; nValue = (int)Keys.OemQuotes; break;
                                        case '<': bShift = true; nValue = (int)Keys.Oemcomma; break;
                                        case '>': bShift = true; nValue = (int)Keys.OemPeriod; break;
                                        case '?': bShift = true; nValue = (int)Keys.OemQuestion; break;

                                        case '!': bShift = true; nValue = (int)Keys.D1; break;
                                        case '@': bShift = true; nValue = (int)Keys.D2; break;
                                        case '#': bShift = true; nValue = (int)Keys.D3; break;
                                        case '$': bShift = true; nValue = (int)Keys.D4; break;
                                        case '%': bShift = true; nValue = (int)Keys.D5; break;
                                        case '^': bShift = true; nValue = (int)Keys.D6; break;
                                        case '&': bShift = true; nValue = (int)Keys.D7; break;
                                        case '*': bShift = true; nValue = (int)Keys.D8; break;
                                        case '(': bShift = true; nValue = (int)Keys.D9; break;
                                        case ')': bShift = true; nValue = (int)Keys.D0; break;

                                        case '`': bShift = false; nValue = (int)Keys.Oemtilde; break;
                                        case '-': bShift = false; nValue = (int)Keys.OemMinus; break;
                                        case '=': bShift = false; nValue = (int)Keys.Oemplus; break;
                                        case '[': bShift = false; nValue = (int)Keys.OemOpenBrackets; break;
                                        case ']': bShift = false; nValue = (int)Keys.OemCloseBrackets; break;
                                        case '\\': bShift = false; nValue = (int)Keys.OemPipe; break;
                                        case ';': bShift = false; nValue = (int)Keys.OemSemicolon; break;
                                        case '\'': bShift = false; nValue = (int)Keys.OemQuotes; break;
                                        case ',': bShift = false; nValue = (int)Keys.Oemcomma; break;
                                        case '.': bShift = false; nValue = (int)Keys.OemPeriod; break;
                                        case '/': bShift = false; nValue = (int)Keys.OemQuestion; break;

                                        case '1': bShift = false; nValue = (int)Keys.D1; break;
                                        case '2': bShift = false; nValue = (int)Keys.D2; break;
                                        case '3': bShift = false; nValue = (int)Keys.D3; break;
                                        case '4': bShift = false; nValue = (int)Keys.D4; break;
                                        case '5': bShift = false; nValue = (int)Keys.D5; break;
                                        case '6': bShift = false; nValue = (int)Keys.D6; break;
                                        case '7': bShift = false; nValue = (int)Keys.D7; break;
                                        case '8': bShift = false; nValue = (int)Keys.D8; break;
                                        case '9': bShift = false; nValue = (int)Keys.D9; break;
                                        case '0': bShift = false; nValue = (int)Keys.D0; break;

                                        case ' ': bShift = false; nValue = (int)Keys.Space; break;
                                        case '\x1b': bShift = false; nValue = (int)Keys.Escape; break;
                                        case '\b': bShift = false; nValue = (int)Keys.Back; break;
                                        case '\t': bShift = false; nValue = (int)Keys.Tab; break;
                                        case '\a': bShift = false; nValue = (int)Keys.LineFeed; break;
                                        case '\r': bShift = false; nValue = (int)Keys.Enter; break;

                                        default:
                                            bShift = false; nValue = 0; break;
                                    }
                                }

                                if (nValue != 0)
                                {
                                    // Caps Lock의 상태에 따른 대/소문자 처리
                                    if (bShift)
                                    {
                                        keybd_event((int)Keys.LShiftKey, 0x00, KEYEVENTF_KEYDOWN, 0);
                                        Thread.Sleep(nDelay);
                                    }

                                    // Key 눌림 처리함.
                                    //int nValue = Convert.ToInt32(chValue);
                                    //int nValue = (int)Keys.Oemtilde;
                                    keybd_event(nValue, 0x00, KEYEVENTF_KEYDOWN, 0);
                                    Thread.Sleep(nDelay);
                                    keybd_event(nValue, 0x00, KEYEVENTF_KEYUP, 0);
                                    Thread.Sleep(nDelay);

                                    // Caps Lock 상태를 회복함.
                                    if (bShift)
                                    {
                                        keybd_event((int)Keys.LShiftKey, 0x00, KEYEVENTF_KEYUP, 0);
                                        Thread.Sleep(nDelay);
                                    }
                                }
                            }
                        }
                    }

                    // 한글 해제
                    if (bHangle)
                    {
                        CommonUtil.keybd_event((int)Keys.HangulMode, 0x00, CommonUtil.KEYEVENTF_KEYDOWN, 0);
                        Thread.Sleep(nDelay);

                        CommonUtil.keybd_event((int)Keys.HangulMode, 0x00, CommonUtil.KEYEVENTF_KEYUP, 0);
                        Thread.Sleep(nDelay);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
        }


        private const int nUnicodeHangleStart = 0xAC00;
        private const int nUnicodeHangleEnd = 0xD79F;
        private static String[] m_astrChoHangle = new String[] { "ㄱ", "ㄲ", "ㄴ", "ㄷ", "ㄸ", "ㄹ", "ㅁ", "ㅂ", "ㅃ", "ㅅ", "ㅆ", "ㅇ", "ㅈ", "ㅉ", "ㅊ", "ㅋ", "ㅌ", "ㅍ", "ㅎ" };
        private static String[] m_astrChoEnglish = new String[] { "r", "R", "s", "e", "E", "f", "a", "q", "Q", "t", "T", "d", "w", "W", "c", "z", "x", "v", "g" };
        private static int m_nNumOfCho = m_astrChoHangle.Length;
        private static String[] m_astrJungHangle = new String[] { "ㅏ", "ㅐ", "ㅑ", "ㅒ", "ㅓ", "ㅔ", "ㅕ", "ㅖ", "ㅗ", "ㅘ", "ㅙ", "ㅚ", "ㅛ", "ㅜ", "ㅝ", "ㅞ", "ㅟ", "ㅠ", "ㅡ", "ㅢ", "ㅣ" };
        private static String[] m_astrJungEnglish = new String[] { "k", "o", "i", "O", "j", "p", "u", "P", "h", "hk", "ho", "hl", "y", "n", "nj", "np", "nl", "b", "m", "ml", "l" };
        private static int m_nNumOfJung = m_astrJungHangle.Length;
        private static String[] m_astrJongHangle = new String[] { "", "ㄱ", "ㄲ", "ㄳ", "ㄴ", "ㄵ", "ㄶ", "ㄷ", "ㄹ", "ㄺ", "ㄻ", "ㄼ", "ㄽ", "ㄾ", "ㄿ", "ㅀ", "ㅁ", "ㅂ", "ㅄ", "ㅅ", "ㅆ", "ㅇ", "ㅈ", "ㅊ", "ㅋ", "ㅌ", "ㅍ", "ㅎ" };
        private static String[] m_astrJongEnglish = new String[] { "", "r", "R", "rt", "s", "sw", "sg", "e", "f", "fr", "fa", "fq", "ft", "fx", "fv", "fg", "a", "q", "qt", "t", "T", "d", "w", "c", "z", "x", "v", "g" };
        private static int m_nNumOfJong = m_astrJongHangle.Length;

        /// <summary>입력 문자가 한글인 경우 자모 분리된 문자을 응답하며, 그렇지 않은 경우 입력 문자를 응답한다,</summary>
        /// <param name="ch">변환할 문자</param>
        /// <param name="bToEnglish">true인 경우 한글 자모를 영문 키 값으로 변환하며, 그렇지 않은 경우 한글 자모를 응답한다.</param>
        /// <returns>ch가 한글인 경우 한글 풀어 쓰기 문자열을 응답하며, 그렇지 않은 경우 ch를 응답한다.</returns>
        public static String[] SplitHangle(Char ch, bool bToEnglish)
        {
            try
            {
                if ((nUnicodeHangleStart <= ch) & (ch <= nUnicodeHangleEnd))
                {
                    int nHangleValue = ch - nUnicodeHangleStart;
                    int nJong = nHangleValue % m_nNumOfJong; nHangleValue /= m_nNumOfJong;
                    int nJung = nHangleValue % m_nNumOfJung; nHangleValue /= m_nNumOfJung;
                    int nCho = nHangleValue % m_nNumOfCho;
                    String[] astrCho, astrJung, astrJong;

                    if (bToEnglish)
                    {
                        astrCho = m_astrChoEnglish;
                        astrJung = m_astrJungEnglish;
                        astrJong = m_astrJongEnglish;
                    }
                    else
                    {
                        astrCho = m_astrChoHangle;
                        astrJung = m_astrJungHangle;
                        astrJong = m_astrJongHangle;
                    }

                    String[] astrHangle = new String[3];
                    astrHangle[0] = astrCho[nCho];
                    astrHangle[1] = astrJung[nJung];
                    astrHangle[2] = astrJong[nJong];

                    return astrHangle;
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }

            return new String[1] { ch.ToString() };
        }
    }
}
