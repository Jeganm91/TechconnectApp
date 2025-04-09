using TechConnect.Models;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using System.Reflection;
//using System.Web.Mvc;
using DA = System.ComponentModel.DataAnnotations;


namespace TechConnect
{
    public class FunctionalTest : IDisposable
    {
        public static List<String> failMessage = new List<String>();
        public static String failureMsg = "";
        public static int failcnt = 1;
        public int totalTestcases = 0;

        public IWebDriver _driver;
        public string userport;
        public string appURL;

        [OneTimeSetUp]
        public void Setup()
        {
            try
            {
                userport = Environment.GetEnvironmentVariable("userport") == null ? "7195" : Environment.GetEnvironmentVariable("userport");

                FirefoxOptions options = new FirefoxOptions();
                // {
                //     AcceptInsecureCertificates = true
                // };

                //options.AddArgument("--headless");
                _driver = new FirefoxDriver(options);

                appURL = "https://localhost:" + userport + "/";
                //_driver = new FirefoxDriver();
                _driver.Navigate().GoToUrl(appURL);
            }
            catch (Exception ex)
            {
                //Console.WriteLine("-----------" + ex.Message);
            }
        }

        public void Dispose()
        {
            if (totalTestcases > 1)
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"./FailedTest.txt"))
                {
                    foreach (string line in failMessage)
                    {
                        //Console.WriteLine("line " + line);
                        file.WriteLine(line);
                    }
                }
            }
            else
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"./FailedTest.txt"))
                {
                    file.WriteLine("error");
                }
            }
            _driver.Quit();
            _driver.Dispose();
        }

        public void ExceptionCatch(string functionName, string catchMsg, string msg, string msg_name, string exceptionMsg = "")
        {
            failMessage.Add(functionName);

            if (msg == "")
            {
                msg = exceptionMsg + (exceptionMsg != "" ? " - " : "") + catchMsg + "\n";
                msg_name += "Fail " + failcnt + " -- " + functionName + "::\n" + msg;
            }
            else
                msg_name += "Fail " + failcnt + " -- " + functionName + "::\n" + msg;

            failureMsg += msg_name;
            failcnt++;
            Assert.Fail(msg);
        }
        public String SeleniumException(IWebDriver wd)
        {
            String msg = "";
            if (wd.Title == "Parser Error")
            {
                string[] stringSeparators = new string[] { "Parser Error Message:" };
                string[] result;
                result = wd.PageSource.Split(stringSeparators, StringSplitOptions.None);
                string[] stringSeparators2 = new string[] { "<b>Source Error:</b>" };
                result = result[1].Split(stringSeparators2, StringSplitOptions.None);
                msg += result[0].Replace("<br>", "").Replace("</b>", "").Replace("\r", "").Replace("\n", "");
            }
            else if (wd.Title.Contains("Error"))
            {
                msg += wd.FindElement(By.CssSelector("h2.exceptionMessage")).Text;
            }
            return msg;
        }

        [Test, Order(1)]
        public void Test1_Check_TechLogin_Properties()
        {
            totalTestcases++;
            String msg = "";
            String msg_name = "";
            string functionName = NUnit.Framework.TestContext.CurrentContext.Test.Name;

            TestBase tb = new TestBase("TechConnect", "TechConnect.Models", "TechLogin");
            var CurrentProperty = new KeyValuePair<string, string>();

            try
            {
                var Properties = new Dictionary<string, string>
                {
                    { "Email", "String" },
                    { "Password", "String" },
                };

                foreach (var property in Properties)
                {
                    CurrentProperty = property;
                    var IsFound = tb.HasProperty(property.Key, property.Value);

                    //Assert.IsTrue(IsFound, tb.Messages.GetPropertyNotFoundMessage(property.Key, property.Value));

                    if (!IsFound)
                    {
                        msg += tb.Messages.GetPropertyNotFoundMessage(property.Key, property.Value) + "\n";
                    }
                }

                if (msg != "")
                {
                    throw new Exception();
                }
            }
            catch (Exception ex)
            {
                //Assert.Fail(tb.Messages.GetExceptionMessage(ex, propertyName: CurrentProperty.Key));
                ExceptionCatch(functionName, tb.Messages.GetExceptionMessage(ex, propertyName: CurrentProperty.Key), msg, msg_name);
            }
        }

        [Test, Order(2)]
        public void Test2_Check_TechLogin_DataAnnotations()
        {
            totalTestcases++;
            String msg = "";
            String msg_name = "";
            string functionName = NUnit.Framework.TestContext.CurrentContext.Test.Name;
            TestBase tb = new TestBase("TechConnect", "TechConnect.Models", "TechLogin");

            //(string propertyname, string attributename) PropertyUnderTest = ("", "");
            string PropertyUnderTest_propertyname = "";
            string PropertyUnderTest_attributename = "";

            try
            {
                //--------------------------------------------
                PropertyUnderTest_propertyname = "Email";
                PropertyUnderTest_attributename = "Required";
                RequiredAttributeTest("Please enter the email");

                PropertyUnderTest_propertyname = "Password";
                PropertyUnderTest_attributename = "Required";
                RequiredAttributeTest("Please enter the password");

                //--------------------------------------------
                PropertyUnderTest_propertyname = "Password";
                PropertyUnderTest_attributename = "DataType";
                DataTypeAttributeTest();

                if (msg != "")
                {
                    throw new Exception();
                }
            }
            catch (Exception ex)
            {
                ExceptionCatch(functionName, $"Exception while testing {PropertyUnderTest_propertyname} for {PropertyUnderTest_attributename} attribute in {tb.type.Name}", msg, msg_name);
            }

            #region LocalFunction_KeyAttributeTest
            void KeyAttributeTest()
            {
                string Message = $"Key attribute on {PropertyUnderTest_propertyname} of {tb.type.Name} is not found";
                var attribute = tb.GetAttributeFromProperty<DA.KeyAttribute>(PropertyUnderTest_propertyname, typeof(DA.KeyAttribute));
                //Assert.IsNotNull(attribute, Message);
                if (attribute == null)
                {
                    msg += Message + "\n";
                }
            }
            #endregion

            #region LocalFunction_RequiredAttributeTest
            void RequiredAttributeTest(string errorMessage)
            {
                string Message = $"Required attribute on {PropertyUnderTest_propertyname} of {tb.type.Name} class doesnot have ";
                var attribute = tb.GetAttributeFromProperty<DA.RequiredAttribute>(PropertyUnderTest_propertyname, typeof(DA.RequiredAttribute));

                if (attribute == null)
                {
                    msg += $"Required attribute not applied on {PropertyUnderTest_propertyname} of {tb.type.Name} class.\n";
                }

                if (errorMessage != attribute.ErrorMessage)
                {
                    msg += $"{Message} ErrorMessage={errorMessage} \n";
                }
            }
            #endregion

            #region LocalFunction_DisplayAttributeTest
            void DisplayAttributeTest(string name)
            {
                string Message = $"Display Attribute on {PropertyUnderTest_propertyname} of {tb.type.Name} class doesnot have ";
                var attribute = tb.GetAttributeFromProperty<DA.DisplayAttribute>(PropertyUnderTest_propertyname, typeof(DA.DisplayAttribute));

                if (name != attribute.Name)
                {
                    msg += $"{Message} Name = {name} \n";
                }
            }
            #endregion

            #region LocalFunction_DataTypeAttributeTest
            void DataTypeAttributeTest()
            {
                //string Message = $"DataType attribute on {PropertyUnderTest_propertyname} of {tb.type.Name} class doesnot have ";
                var attribute = tb.GetAttributeFromProperty<DA.DataTypeAttribute>(PropertyUnderTest_propertyname, typeof(DA.DataTypeAttribute));

                if (attribute.DataType.ToString() != "Password")
                {
                    msg += $"DataType - Password not applied on {PropertyUnderTest_propertyname} of {tb.type.Name} class.\n";
                }
            }
            #endregion
        }

        [Test, Order(3)]
        public void Test3_Check_Registrant_Properties()
        {
            totalTestcases++;
            String msg = "";
            String msg_name = "";
            string functionName = NUnit.Framework.TestContext.CurrentContext.Test.Name;

            TestBase tb = new TestBase("TechConnect", "TechConnect.Models", "Registrant");
            var CurrentProperty = new KeyValuePair<string, string>();

            try
            {
                var Properties = new Dictionary<string, string>
                {
                    { "Id", "Int32" },
                    { "UserName", "String" },
                    { "Password", "String" },
                    { "ConfirmPassword", "String" },
                    { "Email", "String" },
                    { "MobileNumber", "Int64" },

                };

                foreach (var property in Properties)
                {
                    CurrentProperty = property;
                    var IsFound = tb.HasProperty(property.Key, property.Value);

                    //Assert.IsTrue(IsFound, tb.Messages.GetPropertyNotFoundMessage(property.Key, property.Value));

                    if (!IsFound)
                    {
                        msg += tb.Messages.GetPropertyNotFoundMessage(property.Key, property.Value) + "\n";
                    }
                }

                if (msg != "")
                {
                    throw new Exception();
                }
            }
            catch (Exception ex)
            {
                //Assert.Fail(tb.Messages.GetExceptionMessage(ex, propertyName: CurrentProperty.Key));
                ExceptionCatch(functionName, tb.Messages.GetExceptionMessage(ex, propertyName: CurrentProperty.Key), msg, msg_name);
            }
        }

        [Test, Order(4)]
        public void Test4_Check_Registrant_DataAnnotations()
        {
            totalTestcases++;
            String msg = "";
            String msg_name = "";
            string functionName = NUnit.Framework.TestContext.CurrentContext.Test.Name;
            TestBase tb = new TestBase("TechConnect", "TechConnect.Models", "Registrant");

            //modified by Anand Rajan
            //(string propertyname, string attributename) PropertyUnderTest = ("", "");
            string PropertyUnderTest_propertyname = "";
            string PropertyUnderTest_attributename = "";

            try
            {
                PropertyUnderTest_propertyname = "Id";
                PropertyUnderTest_attributename = "Key";
                KeyAttributeTest();

                //--------------------------------------------
                PropertyUnderTest_propertyname = "UserName";
                PropertyUnderTest_attributename = "Required";
                RequiredAttributeTest("Please enter the user name");

                PropertyUnderTest_propertyname = "Password";
                PropertyUnderTest_attributename = "Required";
                RequiredAttributeTest("Please enter the password");

                PropertyUnderTest_propertyname = "ConfirmPassword";
                PropertyUnderTest_attributename = "Required";
                RequiredAttributeTest("Please re-enter the password");

                PropertyUnderTest_propertyname = "Email";
                PropertyUnderTest_attributename = "Required";
                RequiredAttributeTest("Please enter the email");

                PropertyUnderTest_propertyname = "MobileNumber";
                PropertyUnderTest_attributename = "Required";
                RequiredAttributeTest("Please enter the mobile number");

                //--------------------------------------------
                PropertyUnderTest_propertyname = "ConfirmPassword";
                PropertyUnderTest_attributename = "Compare";
                CompareAttributeTest("Password");

                //--------------------------------------------
                PropertyUnderTest_propertyname = "Password";
                PropertyUnderTest_attributename = "DataType";
                DataTypeAttributeTest();

                PropertyUnderTest_propertyname = "ConfirmPassword";
                PropertyUnderTest_attributename = "DataType";
                DataTypeAttributeTest();

                //--------------------------------------------
                //PropertyUnderTest_propertyname = "Age";
                //PropertyUnderTest_attributename = "Range";
                //RangeAttributeTest(19, int.MaxValue, "Age must be greater than 18");

                if (msg != "")
                {
                    throw new Exception();
                }
            }
            catch (Exception ex)
            {
                ExceptionCatch(functionName, $"Exception while testing {PropertyUnderTest_propertyname} for {PropertyUnderTest_attributename} attribute in {tb.type.Name}", msg, msg_name);
            }

            #region LocalFunction_KeyAttributeTest
            void KeyAttributeTest()
            {
                string Message = $"Key attribute on {PropertyUnderTest_propertyname} of {tb.type.Name} is not found";
                var attribute = tb.GetAttributeFromProperty<DA.KeyAttribute>(PropertyUnderTest_propertyname, typeof(DA.KeyAttribute));
                //Assert.IsNotNull(attribute, Message);
                if (attribute == null)
                {
                    msg += Message + "\n";
                }
            }
            #endregion

            #region LocalFunction_RequiredAttributeTest
            void RequiredAttributeTest(string errorMessage)
            {
                string Message = $"Required attribute on {PropertyUnderTest_propertyname} of {tb.type.Name} class doesnot have ";
                var attribute = tb.GetAttributeFromProperty<DA.RequiredAttribute>(PropertyUnderTest_propertyname, typeof(DA.RequiredAttribute));

                if (attribute == null)
                {
                    msg += $"Required attribute not applied on {PropertyUnderTest_propertyname} of {tb.type.Name} class.\n";
                }

                if (errorMessage != attribute.ErrorMessage)
                {
                    msg += $"{Message} ErrorMessage={errorMessage} \n";
                }
            }
            #endregion

            #region LocalFunction_DisplayAttributeTest
            void DisplayAttributeTest(string name)
            {
                string Message = $"Display Attribute on {PropertyUnderTest_propertyname} of {tb.type.Name} class doesnot have ";
                var attribute = tb.GetAttributeFromProperty<DA.DisplayAttribute>(PropertyUnderTest_propertyname, typeof(DA.DisplayAttribute));

                if (name != attribute.Name)
                {
                    msg += $"{Message} Name = {name} \n";
                }
            }
            #endregion

            #region LocalFunction_CompareAttributeTest
            void CompareAttributeTest(string name)
            {
                string Message = $"Compare Attribute on {PropertyUnderTest_propertyname} of {tb.type.Name} class doesnot have ";
                var attribute = tb.GetAttributeFromProperty<DA.CompareAttribute>(PropertyUnderTest_propertyname, typeof(DA.CompareAttribute));

                if (name != attribute.OtherProperty)
                {
                    msg += $"{Message} Compare = {name} \n";
                }
            }
            #endregion

            #region LocalFunction_DataTypeAttributeTest
            void DataTypeAttributeTest()
            {
                //string Message = $"DataType attribute on {PropertyUnderTest_propertyname} of {tb.type.Name} class doesnot have ";
                var attribute = tb.GetAttributeFromProperty<DA.DataTypeAttribute>(PropertyUnderTest_propertyname, typeof(DA.DataTypeAttribute));

                if (attribute.DataType.ToString() != "Password")
                {
                    msg += $"DataType - Password not applied on {PropertyUnderTest_propertyname} of {tb.type.Name} class.\n";
                }
            }
            #endregion

            #region LocalFunction_RangeAttributeTest
            void RangeAttributeTest(int min, int max, string errorMessage)
            {
                string Message = $"Range attribute on {PropertyUnderTest_propertyname} of {tb.type.Name} class doesnot have ";
                var attribute = tb.GetAttributeFromProperty<DA.RangeAttribute>(PropertyUnderTest_propertyname, typeof(DA.RangeAttribute));

                if (Convert.ToInt32(attribute.Minimum) != min || Convert.ToInt32(attribute.Maximum) != max)
                {
                    msg += $"Range not applied on {PropertyUnderTest_propertyname} of {tb.type.Name} class.\n";
                }

                if (errorMessage != attribute.ErrorMessage)
                {
                    msg += $"{Message} ErrorMessage={errorMessage} \n";
                }
            }
            #endregion
        }

        [Test, Order(5)]
        public void Test5_TechDBContext_DbSet_Property_CreationTest()
        {
            totalTestcases++;
            String msg = "";
            String msg_name = "";
            string functionName = NUnit.Framework.TestContext.CurrentContext.Test.Name;
            TestBase tb = new TestBase("TechConnect", "TechConnect.Models", "TechDBContext");
            try
            {
                var IsFound = tb.HasProperty("Registrants", "DbSet`1");
                if (!IsFound)
                {
                    msg += tb.Messages.GetPropertyNotFoundMessage("Registrants", "DbSet<Registrant> \n");
                }

                if (msg != "")
                {
                    throw new Exception();
                }
            }
            catch (Exception ex)
            {
                //Assert.Fail(tb.Messages.GetExceptionMessage(ex, propertyName: "Donors"));
                ExceptionCatch(functionName, tb.Messages.GetExceptionMessage(ex), msg, msg_name);
            }
        }

        [Test, Order(6)]
        [TestCase("UserLogin", TestName = "Test6_UserLogin_IsAvailable")]
        [TestCase("AddRegistrant", TestName = "Test7_AddRegistrant_IsAvailable")]
        [TestCase("End", TestName = "Test8_End_IsAvailable")]
        public void Test6_7_8_Get_ActionCreated_Test(string mname)
        {
            totalTestcases++;
            String msg = "";
            String msg_name = "";
            string functionName = NUnit.Framework.TestContext.CurrentContext.Test.Name;
            TestBase tb = new TestBase("TechConnect", "TechConnect.Controllers", "LotteryController");
            try
            {
                var Method = tb.type.GetMethod(mname, new Type[] { });

                if (Method == null)
                {
                    msg += $"{tb.type.Name} doesnot defines action method \n";
                }

                if (msg != "")
                {
                    throw new Exception();
                }
            }
            catch (Exception ex)
            {
                ExceptionCatch(functionName, $"Exception while check Get action method is present or not in {tb.type.Name}. \n", msg, msg_name);
            }
        }

        [Test, Order(9)]
        public void Test9_UserLogin_Post_ActionCreated_Test()
        {
            totalTestcases++;
            String msg = "";
            String msg_name = "";
            string functionName = NUnit.Framework.TestContext.CurrentContext.Test.Name;

            TestBase tb = new TestBase("TechConnect", "TechConnect.Controllers", "LotteryController");
            try
            {
                var Method = tb.type.GetMethod("UserLogin", new Type[] { typeof(TechLogin) });
                if (Method == null)
                {
                    msg += $"{tb.type.Name} doesnot defines create action method which accepts over object as parameter \n";
                }

                var attr = Method.GetCustomAttribute<HttpPostAttribute>();

                if (attr == null)
                {
                    msg += $"create action is not marked with attributes to run on http post request in {tb.type.Name} controller \n";
                }

                if (msg != "")
                {
                    throw new Exception();
                }
            }
            catch (Exception ex)
            {
                ExceptionCatch(functionName, $"Exception while check UserLogin action method is present or not in {tb.type.Name}. \n", msg, msg_name);
            }
        }

        [Test, Order(10)]
        public void Test10_AddRegistrant_Post_ActionCreated_Test()
        {
            totalTestcases++;
            String msg = "";
            String msg_name = "";
            string functionName = NUnit.Framework.TestContext.CurrentContext.Test.Name;

            TestBase tb = new TestBase("TechConnect", "TechConnect.Controllers", "LotteryController");
            try
            {
                var Method = tb.type.GetMethod("AddRegistrant", new Type[] { typeof(Registrant) });
                if (Method == null)
                {
                    msg += $"{tb.type.Name} doesnot defines create action method which accepts over object as parameter \n";
                }

                var attr = Method.GetCustomAttribute<HttpPostAttribute>();

                if (attr == null)
                {
                    msg += $"create action is not marked with attributes to run on http post request in {tb.type.Name} controller \n";
                }

                if (msg != "")
                {
                    throw new Exception();
                }
            }
            catch (Exception ex)
            {
                ExceptionCatch(functionName, $"Exception while check AddRegistrant action method is present or not in {tb.type.Name}. \n", msg, msg_name);
            }
        }

        [Test, Order(11)]
        public void Test11_UI_AddRegistrant_Message()
        {
            totalTestcases++;
            String msg = "";
            String msg_name = "";
            string functionName = NUnit.Framework.TestContext.CurrentContext.Test.Name;

            try
            {

                _driver.Navigate().GoToUrl(appURL + "Lottery/AddRegistrant");
                System.Threading.Thread.Sleep(5000);
                //msg+="========="+_driver.PageSource.ToString();

                _driver.SetElementText("UserName", "TestName");
                _driver.SetElementText("Password", "testname@test.com");
                _driver.SetElementText("ConfirmPassword", "testname@test.com");
                _driver.SetElementText("Email", "testname@test.com");
                _driver.SetElementText("MobileNumber", "8098889990");
                _driver.ClickElement("btnCreate");
                System.Threading.Thread.Sleep(5000);

                //  msg+="========="+_driver.PageSource.ToString();
                // var result = _driver.FindElement(By.Id("check"));
                // msg+="==="+result.ToString();
                
                if (!_driver.PageSource.Contains("Email Id and Password should not be the same"))
                {
                    msg += $"AddRegistrant Page NOT displaying the message for Invalid Password correctly.\n";
                }

                if (msg != "")
                {
                    throw new Exception();
                }
            }
            catch (Exception ex)
            {
                ExceptionCatch(functionName, ex.Message + $"-Exception trying to add and display the registrant details. Exception :{ex.InnerException?.Message}\n", msg, msg_name);
            }
        }

        [Test, Order(12)]
        public void Test12_UI_Test()
        {
            totalTestcases++;
            String msg = "";
            String msg_name = "";
            string functionName = NUnit.Framework.TestContext.CurrentContext.Test.Name;

            try
            {

                _driver.Navigate().GoToUrl(appURL + "Lottery/UserLogin");
                System.Threading.Thread.Sleep(5000);
                //  msg+="+++++++++"+_driver.PageSource.ToString();
                _driver.FindElement(By.Id("lnkRegister")).Click();


                _driver.SetElementText("UserName", "TestName");
                _driver.SetElementText("Password", "12345");
                _driver.SetElementText("ConfirmPassword", "12345");
                _driver.SetElementText("Email", "testname@test.com");
                _driver.SetElementText("MobileNumber", "8098889990");
                _driver.ClickElement("btnCreate");
                System.Threading.Thread.Sleep(5000);


                //Console.WriteLine(_driver.PageSource);
                _driver.Navigate().GoToUrl(appURL + "Lottery/UserLogin");
                System.Threading.Thread.Sleep(5000);

                _driver.SetElementText("Email", "testname@test.com");
                _driver.SetElementText("Password", "12345");

                _driver.ClickElement("btnSubmit");
                System.Threading.Thread.Sleep(5000);
                //msg+=_driver.PageSource.ToString();
                var result = _driver.FindElement(By.Id("message"));

                if (!result.Text.ToString().ToLower().Contains("registration") || !result.Text.ToString().ToLower().Contains("successfully") ||
                        !result.Text.ToString().ToLower().Contains("completed") || !result.Text.ToString().Contains("TestName"))
                {
                    msg += $"End Page NOT displaying the details correctly.\n";
                }

                if (msg != "")
                {
                    throw new Exception();
                }
            }
            catch (Exception ex)
            {
                ExceptionCatch(functionName, ex.Message + $"-Exception trying to add and display the user details. Exception :{ex.InnerException?.Message}\n", msg, msg_name);
            }
        }
    }

    public class AssertFailureMessages
    {
        private string TypeName;
        public AssertFailureMessages(string typeName)
        {
            this.TypeName = typeName;
        }
        public string GetAssemblyNotFoundMessage(string assemblyName)
        {
            return $"Could not find {assemblyName}.dll";
        }
        public string GetTypeNotFoundMessage(string assemblyName, string typeName = null)
        {
            return $"Could not find {typeName ?? TypeName} in  {assemblyName}.dll";
        }
        public string GetFieldNotFoundMessage(string fieldName, string fieldType, string typeName = null)
        {
            return $"Could not a find public field {fieldName} of {fieldType} type in {typeName ?? TypeName} class";
        }
        public string GetPropertyNotFoundMessage(string propertyName, string propertyType, string typeName = null)
        {
            return $"Could not a find public property {propertyName} of {propertyType} type in {typeName ?? TypeName} class";
        }
        public string GetFieldTypeMismatchMessage(string fieldName, string expectedFieldType, string typeName = null)
        {
            return $"{fieldName} is not of {expectedFieldType} data type in {typeName ?? TypeName} class";
        }
        public string GetExceptionTestFailureMessage(string methodName, string customExceptionTypeName, string propertyName, Exception exception, string typeName = null)
        {
            return $"{methodName} method of {typeName ?? TypeName} class doesnot throws exception of type {customExceptionTypeName} on validation failure for {propertyName}.\nException Message: {exception.InnerException?.Message}\nStack Trace:{exception.InnerException?.StackTrace}";
        }

        public string GetExceptionMessage(Exception ex, string methodName = null, string fieldName = null, string propertyName = null, string typeName = null)
        {
            string testFor = methodName != null ? methodName + " method" : fieldName != null ? fieldName + " field" : propertyName != null ? propertyName + " property" : "undefined";
            //return $" Exception while testing {testFor} of {typeName ?? TypeName} class.\nException message : {ex.InnerException?.Message}\nStack Trace : {ex.InnerException?.StackTrace}";
            return $" Exception while testing {testFor} of {typeName ?? TypeName} class.\n";
        }

        public string GetReturnTypeAssertionFailMessage(string methodName, string expectedTypeName, string typeName = null)
        {
            return $"{methodName} method of {typeName ?? TypeName} class doesnot return value of {expectedTypeName} data type";
        }
        public string GetReturnValueAssertionFailMessage(string methodName, object expectedValue, string typeName = null)
        {
            return $"{methodName} method of {typeName ?? TypeName} class doesnot return the value {expectedValue}";
        }

        public string GetValidationFailureMessage(string methodName, string expectedValidationMessage, string propertyName, string typeName = null)
        {
            return $"{methodName} method of {typeName ?? TypeName} class doesnot return '{expectedValidationMessage}' on validation failure for property {propertyName}";
        }

    }

    public static class SeleniumExtensions
    {

        public static void SetElementText(this IWebDriver driver, string elementId, string text)
        {
            var Element = driver.FindElement(By.Id(elementId));
            Element.Clear();
            Element.SendKeys(text);
        }

        public static string GetElementText(this IWebDriver driver, string elementId)
        {
            return driver.GetElementText(elementId);
        }

        public static void ClickElement(this IWebDriver driver, string elementId)
        {
            driver.FindElement(By.Id(elementId)).Click();
        }

        //public static void SelectDropDownItemByValue(this IWebDriver driver, string elementId, string value)
        //{
        //    new SelectElement(driver.FindElement(By.Id(elementId))).SelectByValue(value);
        //}
        //public static void SelectDropDownItemByText(this IWebDriver driver, string elementId, string text)
        //{
        //    new SelectElement(driver.FindElement(By.Id(elementId))).SelectByText(text);
        //}


        public static string GetElementInnerText(this IWebDriver driver, string elementType, string attribute)
        {
            return driver.FindElement(By.XPath($"//{elementType}[{attribute}]")).GetAttribute("innerHTML");
        }

        public static int GetTableRowsCount(this IWebDriver driver, string elementId)
        {
            var Table = driver.FindElement(By.Id(elementId));
            return Table.FindElements(By.TagName("tr")).Count;
        }



    }

    public class TestBase : ATestBase
    {
        public TestBase(string assemblyName, string namespaceName, string typeName)
        {
            Console.WriteLine("-----12-------");
            Messages = new AssertFailureMessages(typeName);
            this.assemblyName = assemblyName;
            this.namespaceName = namespaceName;
            this.typeName = typeName;

            Console.WriteLine("-----13-------");
            Messages = new AssertFailureMessages(typeName);
            Console.WriteLine("-----14-------");
            assembly = Assembly.Load(assemblyName);
            Console.WriteLine("-----15-------");
            type = assembly.GetType($"{namespaceName}.{typeName}");
            Console.WriteLine("-----16-------");
        }
    }
    public abstract class ATestBase
    {
        public string assemblyName;
        public string namespaceName;
        public string typeName;
        public string controllerName;

        public AssertFailureMessages Messages;//= new AssertFailureMessages(typeName);

        protected Assembly assembly;
        public Type type;


        protected object typeInstance = null;
        protected void CreateNewTypeInstance()
        {
            typeInstance = assembly.CreateInstance(type.FullName);
        }
        public object GetTypeInstance()
        {
            if (typeInstance == null)
                CreateNewTypeInstance();
            return typeInstance;
        }
        public object InvokeMethod(string methodName, Type type, params object[] parameters)
        {
            var method = type.GetMethod(methodName);
            var instance = GetTypeInstance();
            var result = method.Invoke(instance, parameters);
            return result;
        }
        public T InvokeMethod<T>(string methodName, Type type, params object[] parameters)
        {
            var result = InvokeMethod(methodName, type, parameters);
            return (T)Convert.ChangeType(result, typeof(T));
        }

        public bool HasField(string fieldName, string fieldType)
        {
            bool Found = false;
            var field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            if (field != null)
            {
                Found = field.FieldType.Name == fieldType;
            }
            return Found;
        }

        public bool HasProperty(string propertyName, string propertyType)
        {
            bool Found = false;
            var property = type.GetProperty(propertyName);
            if (property != null)
            {
                Found = property.PropertyType.Name == propertyType; ;
            }
            return Found;
        }

        public T GetAttributeFromProperty<T>(string propertyName, Type attribute)
        {

            var attr = type.GetProperty(propertyName).GetCustomAttribute(attribute, false);
            return (T)Convert.ChangeType(attr, typeof(T));
        }

        //public bool CheckFromUriAttribute(string methodName)
        //{
        //    ParameterInfo[] parameters = type.GetMethod(methodName).GetParameters();
        //    if (parameters.Length == 0)
        //    {
        //        return false;
        //    }

        //    Object[] myAttributes = parameters[0].GetCustomAttributes(typeof(System.Web.Http.FromUriAttribute), false);
        //    //{System.Web.Http.FromUriAttribute[0]}
        //    if (myAttributes.Length == 0)
        //    {
        //        return false;
        //    }
        //    else
        //    {
        //        return true;
        //    }
        //}

        //public bool CheckFromBodyAttribute(string methodName)
        //{
        //    ParameterInfo[] parameters = type.GetMethod(methodName).GetParameters();
        //    if (parameters.Length == 0)
        //    {
        //        return false;
        //    }

        //    Object[] myAttributes = parameters[0].GetCustomAttributes(typeof(System.Web.Http.FromBodyAttribute), false);

        //    if (myAttributes.Length == 0)
        //    {
        //        return false;
        //    }
        //    else
        //    {
        //        return true;
        //    }
        //}
    }
}
