using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using ModelClasses.Entities.TestParts;
using QuizApp.ViewModel.Managing;
using QuizApp.ViewModel.Mapping;
using Services;
using WebGrease.Css.Ast.Selectors;

namespace QuizApp.Controllers
{
    public class TestingController : Controller
    {
        private readonly IGetInfoService _getInfoService;
        private readonly ILowLevelTestManagementService _lowLevelTestManagementService;
        private readonly IHighLevelTestManagementService _highLevelTestManagementService;

        private readonly IMapper _mapper;
        private readonly IAdvancedMapper _advancedMapper;

        public TestingController(IGetInfoService getInfoService,
            ILowLevelTestManagementService lowLevelTestManagementService,
            IHighLevelTestManagementService highLevelTestManagementService, IMapper mapper,
            IAdvancedMapper advancedMapper)
        {
            _getInfoService = getInfoService;
            _lowLevelTestManagementService = lowLevelTestManagementService;
            _highLevelTestManagementService = highLevelTestManagementService;
            _mapper = mapper;
            _advancedMapper = advancedMapper;
        }

        [HttpGet]
        public ActionResult GetAnswersByQuestionGuide(string questionGuid)
        {

            var answerViewModelList = _getInfoService
                .GetQuestionByGuid(questionGuid)
                ?.TestAnswers
                .Select(a => _mapper.Map<AnswerViewModel>(a))
                .ToList();
            return View(answerViewModelList.ToString());
        }

        [HttpGet]
        public ActionResult CreateAnswer(string questionGuid)
        {
            if (questionGuid != null)
            {
                var model = new AnswerViewModel
                {
                    Guid = questionGuid
                };
                return View(model);
            }

            return HttpNotFound();
        }
        [HttpPost]
        public ActionResult CreateAnswer(string questionGuid, AnswerViewModel answer)
        {
            if (!ModelState.IsValid)
                return View(answer);
            var testAnswer = _mapper.Map<TestAnswer>(answer);
            _lowLevelTestManagementService.CreateAnswerForQuestion(answer.Guid, testAnswer);

            return RedirectToAction("GetQuestionsByTestGuid", "Testing", new { testGuid = questionGuid });

        }

        [HttpGet]
        public ActionResult RemoveAnswer(string answerGuid)
        {
            if (answerGuid != null)
            {
                _lowLevelTestManagementService.RemoveAnswer(answerGuid);
                return RedirectToAction("TestManagement", "Admin");
            }

            return HttpNotFound();


        }

        [HttpGet]
        public ActionResult GetQuestionsByTestGuid(string testGuid)
        {
            if (testGuid != null)
            {
                var questionViewModelList = _getInfoService
                    .GetTestByGuid(testGuid)
                    ?.TestQuestions
                    .Select(q => _advancedMapper.MapTestQuestion(q))
                    .ToList();

                return View(questionViewModelList);
            }
            return RedirectToAction("TestManagement", "Admin");
        }
        [HttpGet]
        public ActionResult CreateQuestion(string testGuid)
        {
            if (testGuid != null)
            {
                var model = new QuestionViewModel
                {

                    Guid = testGuid

                };
                return View(model);
            }
            return HttpNotFound();
        }
        [HttpPost]
        public ActionResult CreateQuestion(string testGuid, QuestionViewModel question)
        {
            if (ModelState.IsValid)
            {
                if (testGuid != null && question != null)
                {
                    var testQuestion = _mapper.Map<TestQuestion>(question);
                    _lowLevelTestManagementService.CreateQuestionForTest(testGuid, testQuestion);

                    return RedirectToAction("GetQuestionsByTestGuid", "Testing",
                        new
                        {
                            TestGuid = testGuid
                        });
                }
                return HttpNotFound();
            }
            return View(question);
        }

        [HttpGet]
        public ActionResult RemoveQuestion(string questionGuid)
        {
            _lowLevelTestManagementService.RemoveQuestion(questionGuid);
            return RedirectToAction("TestManagement", "Admin");

        }
        [HttpGet]
        public ActionResult UpdateQuestion(string questionGuid)
        {
            var testQuestion = _advancedMapper.MapTestQuestion(_getInfoService.GetQuestionByGuid(questionGuid));


            if (testQuestion != null)
            {


                var model = new QuestionViewModel
                {
                    Guid = questionGuid
                };
                return View(model);
            }

            return HttpNotFound();
        }


        [HttpPost]
        public ActionResult UpdateQuestion(string questionGuid, QuestionViewModel question)
        {
            if (ModelState.IsValid)
            {
                var testQuestion = _mapper.Map<TestQuestion>(question);
                if (testQuestion != null)
                {
                    _lowLevelTestManagementService.UpdateQuestion(question.Guid, testQuestion);
                    return RedirectToAction("GetQuestionsByTestGuid", "Testing", routeValues: new { testGuid = Session["testGuid"] });
                }
                return HttpNotFound();
            }

            return View(question);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>

        [HttpGet]
        public ActionResult CreateTest()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreateTest(TestViewModel test)
        {
            if (ModelState.IsValid)
            {
                if (test.TestTimeLimit == null)
                    test.TestTimeLimit = new TimeSpan().ToString();
                if (test.QuestionTimeLimit == null)
                    test.QuestionTimeLimit = new TimeSpan().ToString();
                var testFromDomain = _advancedMapper.MapTestViewModel(test);
                _highLevelTestManagementService.CreateTest(testFromDomain);
                if (testFromDomain != null)
                {
                    return RedirectToAction("TestManagement", "Admin");
                }

                return View(test);

            }

            return View(test);
        }
        [HttpGet]
        public ActionResult UpdateTest(string testGuid)
        {
            Session["testGuid"] = testGuid;
            TestViewModel test = _advancedMapper.MapTest(_getInfoService.GetTestByGuid(testGuid));
            if (test != null)

            {
                return View(test);
            }

            return HttpNotFound();

        }

        [HttpPost]
        public ActionResult UpdateTest(string testGuid, TestViewModel test)
        {
            if (ModelState.IsValid)
            {
                if (testGuid != null)
                {
                    if (test.TestTimeLimit == null)
                        test.TestTimeLimit = new TimeSpan().ToString();
                    if (test.QuestionTimeLimit == null)
                        test.QuestionTimeLimit = new TimeSpan().ToString();
                    var testFromDomain = _advancedMapper.MapTestViewModel(test);
                    if (testFromDomain != null)
                    {
                        _highLevelTestManagementService.UpdateTest(testGuid, testFromDomain);
                        return RedirectToAction("TestManagement", "Admin");
                    }
                }

                return HttpNotFound();

            }
            return View(test);
        }

        [HttpGet]
        public ActionResult RemoveTest(string testGuid)
        {
            if (testGuid != null)
            {
                _highLevelTestManagementService.RemoveTest(testGuid);
                return RedirectToAction("TestManagement", "Admin");
            }
            return HttpNotFound();
        }

        [HttpGet]
        public ActionResult CreateTestingUrl(string testGuid)
        {
            var model = new TestingUrlViewModel
            {
                Guid = testGuid
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult CreateTestingUrl(TestingUrlViewModel model)
        {
            if (model != null)
            {
                if (model.AllowedEndDate == null)
                    model.AllowedEndDate = new DateTime().ToString();
                if (model.AllowedStartDate == null)
                    model.AllowedStartDate = new DateTime().ToString();
            }
            if (ModelState.IsValid)
            {
                var testUrlDomain = _advancedMapper.MapTestingUrlViewModel(model);
                _highLevelTestManagementService.CreateTestingUrl(testUrlDomain);
                return RedirectToAction("TestingUrlManagement", "Admin");
            }

            return View(model);
        }
        [HttpPost]
        public void RemoveTestingUrl(string testingUrlGuid)
        {
            _highLevelTestManagementService.RemoveTestingUrl(testingUrlGuid);
        }

        [HttpPost]
        public void RemoveTestingResult(string testingResultGuid)
        {
            _highLevelTestManagementService.RemoveTestingResult(testingResultGuid);
        }
    }
}