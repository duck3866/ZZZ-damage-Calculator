using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using Newtonsoft.Json;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;
using System.Diagnostics.Tracing;
using System.Threading;

namespace scouter
{
    public partial class Form1 : Form
    {
        static string json = @"
        {
           ""characters"": [
             {
               ""type"": ""강공"",
               ""members"": [""주연"", ""엘렌"", ""앤톤"", ""11호"", ""코린"", ""빌리"", ""네코마타"", ""하루마사""]
             },
             {
               ""type"": ""이상"",
               ""members"": [""야나기"", ""제인"", ""버니스"", ""파이퍼"", ""그레이스"", ""미야비""]
             },
             {
               ""type"": ""격파"",
               ""members"": [""리카온"", ""청의"", ""라이터"", ""콜레다"", ""엔비""]
             },
             {
               ""type"": ""지원"",
               ""members"": [""니콜"", ""루시"", ""리나"", ""소우카쿠""]
             },  
             {
               ""type"": ""방어"",
               ""members"": [""카이사르"", ""세스"", ""벤""]
             }
           ]
        }";

        // JSON 파싱
        

        List<string> CharacterList = new List<string>
        {
            "네코마타",
            "11호",
            "코린",
            "빌리",
            "앤톤",
            "하루마사",
            "콜레다",
            "리카온",
            "엔비",
            "라이터",
            "그레이스",
            "파이퍼",
            "미야비",
            "리나",
            "니콜",
            "소우카쿠",
            "루시",
            "벤",
            "세스",
            "카이사르",
            "버니스",
            "주연",
            "리카온",
            "청의",
            "제인",
            "엘렌",
            "야나기"
        };

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateRoundRectRgn(
               int x1, int y1, int x2, int y2,int cx, int cy
            );
        [DllImport("user32.dll")]
        private static extern int SetWindowRgn(IntPtr hWnd,IntPtr hRgn, bool bRedraw);

        public Form1()
        {
            InitializeComponent();
            foreach (var character in CharacterList)
            {
                characterList.Items.Add(character);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            IntPtr ip = CreateRoundRectRgn(0, 0, panel1.Width, panel1.Height, 25, 25);
            int i = SetWindowRgn(panel1.Handle, ip, true);
            IntPtr ip2 = CreateRoundRectRgn(0, 0, panel2.Width, panel2.Height, 50, 50);
            int i2 = SetWindowRgn(panel2.Handle, ip2, true);
        }
        private void CalculateDamage(
                 double baseAttack,
                 double attackPowerPercent,
                 double flatAttack,
                 double damageIncrease,
                 double elementalDamage,
                 double penetrationPercent,
                 double penetrationFlat,
                 double critRate,
                 double critDamage,
                 double monsterDefense,
                 double monsterResist,
                 double weaknessMult
 )
        {
            try
            {
                // 1. 최종 공격력 계산
                double totalAttackPower = (baseAttack * (1 + attackPowerPercent / 100)) + flatAttack;
      

                // 2. 피해 증가 계산
                double totalDamageIncrease = 1 + (damageIncrease / 100) + (elementalDamage / 100);
             

                // 3. 치명타 기댓값 계산
                double critExpected = 1 + (Math.Min(critRate, 100) / 100) * (critDamage / 100);
                

                // 4. 방어력 계산
                double finalDefense = monsterDefense * (1 - penetrationPercent / 100) - penetrationFlat;
                double defenseMultiplier = 1 - (finalDefense / (finalDefense + 1000));
              
                // 5. 저항 계산
                double resistMultiplier = 1 - (monsterResist / 100);
            

                // 6. 그로기 배율
                double weaknessMultiplier = weaknessMult / 100;
          
                // 최종 데미지 계산
                double finalDamage = totalAttackPower *
                                   totalDamageIncrease *
                                   critExpected *
                                   defenseMultiplier *
                                   resistMultiplier *
                                   weaknessMultiplier;

           
                Result.Text = finalDamage.ToString("F2");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"계산 중 오류: {ex.Message}");
                throw;
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            try
            {
                Console.WriteLine("계산 시작");
                double baseAttack = double.Parse(Damage.Text);
                double critDamage = double.Parse(criDamage.Text);
                double critChance = double.Parse(cri.Text);
                double penetrationPercent = double.Parse(penetration.Text);
                double elementalDamage = double.Parse(elementaldamage.Text);
                double penetrationFlat = double.Parse(textBox1.Text);
                double weaknessMult = double.Parse(textBox2.Text);

                CalculateDamage(
                    baseAttack,           // 기본 공격력
                    0,                    // 공격력 증가%
                    0,                    // 플랫 공격력
                    0,                    // 피해 증가
                    elementalDamage,      // 속성 피해 증가
                    penetrationPercent,   // 관통률
                    penetrationFlat,      // 관통수치
                    critChance,          // 치명타 확률
                    critDamage,          // 치명타 피해
                    100,                 // 몬스터 방어력
                    0,                   // 몬스터 저항
                    weaknessMult         // 그로기 약체화 배율
                );
                String panelText = OnChangePanelText();
                String textAnimation = "";
                for (int i = 0; panelText.Length > i; i++)
                {
                    textAnimation += panelText[i];
                    recomend.Text = textAnimation;
                    await Task.Delay(100);
                }
               
            }
            catch (FormatException ex)
            {
                MessageBox.Show("유효한 숫자를 입력하세요");
                Console.WriteLine($"형식 오류: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"오류 발생: {ex.Message}");
                Console.WriteLine($"일반 오류: {ex.Message}");
            }
        }
        private String OnChangePanelText()
        {
            Root root = JsonConvert.DeserializeObject<Root>(json);
            String findText = characterList.Text;

            var resultText = root.Characters.FirstOrDefault(c=>c.members.Contains(findText));
            if (resultText != null)
            { //타입에 맞춘 텍스트 제공
                if (resultText.type == "강공")
                {
                    if (int.Parse(cri.Text) < 50)
                    {
                        return "치명타 확률을 올리세요";
                    }
                    else if(int.Parse(criDamage.Text) < 100)
                    {
                        return "치명타 데미지를 올리세요";
                    }
                    else
                    {
                        return "공격력이나 올리세요";
                    }
                    
                }
                else if (resultText.type == "격파")
                {
                    if(int.Parse(textBox2.Text) < 160)
                    {
                        return "충격력을 올리세요";
                    }
                    else if (int.Parse(Damage.Text) < 2000)
                    {
                        return "공격력을 올리세요";
                    }
                    else
                    {
                        return "치명타 확률/데미지를 올리세요";
                    }
                } 
                else if (resultText.type == "이상")
                {
                    if(int.Parse(Damage.Text) < 3000)
                    {
                        return "공격력을 올리세요";
                    }
                    else
                    {
                        return "이상력을 올리세요";
                    }
                }
                else if (resultText.type == "지원")
                {
                    if (int.Parse(Damage.Text) < 1500)
                    {
                        return "공격력을 올리세요";
                    }
                    else
                    {
                        return "에너지 회복 효율/이상 수치를 올리세요";
                    }
                }
                else if(resultText.type == "방어")
                {
                    if(int.Parse(textBox2.Text) < 150)
                    {
                        return "충격력이나 올리세요";
                    }
                    else
                    {
                        return "공격력이나 올리세요";
                    }
                }
                else
                {
                    return findText + "는 공격력이나 올리세요";
                }
            }
           
            else
            {
                return "캐릭터를 선택하세요";
            }
           
        }
        

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void Damage_TextChanged(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void label15_Click(object sender, EventArgs e)
        {

        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void label11_Click(object sender, EventArgs e)
        {

        }

        private void label14_Click(object sender, EventArgs e)
        {

        }

        private void label13_Click(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void label12_Click(object sender, EventArgs e)
        {

        }

        private void label16_Click(object sender, EventArgs e)
        {

        }

        private void panel5_Paint(object sender, PaintEventArgs e)
        {
          
        }

        private void characterList_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void panel6_Paint(object sender, PaintEventArgs e)
        {

        }

        private void Result_TextChanged(object sender, EventArgs e)
        {

        }
    }
    public class Character
    {
        public string type { get; set; }
        public List<string> members { get; set; }
    }
    public class Root
    {
        public List<Character> Characters { get; set; }
    }
}
