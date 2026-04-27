using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Net;
using GeniView.Cloud.Repository;
using GeniView.Data.Hardware;
using GeniView.Data.Web;
using GeniView.Cloud.Models;
using NLog;

namespace GeniView.Cloud.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Application Admin")]
    public class GroupsController : Controller
    {
        private readonly GroupsDataRepository groupRepo;
        private readonly CommunitiesDataRepository communityRepo;
        private UserActivityHistory userAHM = new UserActivityHistory();
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly IdentityDataRepository _identityRepo;

        public GroupsController(GroupsDataRepository groupRepo, CommunitiesDataRepository communityRepo, IdentityDataRepository identityRepo)
        {
            this.groupRepo = groupRepo;
            this.communityRepo = communityRepo;
            _identityRepo = identityRepo;
        }

        private void PopulateDropdowns(long? selectedCommunityID = null)
        {
            var repo = communityRepo;
            {
                ViewBag.Communities = new SelectList(repo.GetCommunities(), "ID", "Name", selectedCommunityID);
            }
            ViewBag.Groups = new SelectList(groupRepo.GetGroups(selectedCommunityID), "ID", "Name");
        }

        public ActionResult Index()
        {
            try
            {
                var identityRepo = _identityRepo;
                {
                    ViewBag.CurrentUser = identityRepo.FindUserByID(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
                }
                var model = groupRepo.GetGroups();
                return View(model);

            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Geni-View Cloud encountered an error.");
                ModelState.AddModelError("DbFail", ex.Message);
                return View();
            }
        }

        public ActionResult Create()
        {
            PopulateDropdowns();
            return View(new GroupViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind("ParentGroupID,CommunityID,Group")] GroupViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    groupRepo.InsertGroup(model);
                    userAHM.AddActivity("Create new Group", ActivityObjectType.Group, model.Group.Name);
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Geni-View Cloud encountered an error.");
                    ModelState.AddModelError("DbFail", ex.Message);
                }
            }
            PopulateDropdowns(model.CommunityID);
            return View(model);
        }

        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return StatusCode((int)System.Net.HttpStatusCode.BadRequest);
            }

            GroupViewModel model = new GroupViewModel();

            try
            {
                model = groupRepo.FindGroupByID(id.Value);

                if (model == null)
                    return NotFound();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Geni-View Cloud encountered an error.");
                ModelState.AddModelError("DbFail", ex.Message);
                return View(model);
            }
            PopulateDropdowns(model.CommunityID);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(GroupViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Group cannot be group own parent
                    if (model.Group.ID == model.ParentGroupID)
                    {
                        ModelState.AddModelError("DbFail", "Group cannot be own parent Group");
                        return View(model);
                    }

                    groupRepo.UpdateGroup(model);
                    userAHM.AddActivity("Edit Group", ActivityObjectType.Group, model.Group.Name);
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Geni-View Cloud encountered an error.");
                    ModelState.AddModelError("DbFail", ex.Message);
                }
            }
            PopulateDropdowns(model.CommunityID);
            return View(model);
        }

        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return StatusCode((int)System.Net.HttpStatusCode.BadRequest);
            }
            Group group = new Group();
            try
            {
                group = groupRepo.FindGroupByID(id.Value).Group;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Geni-View Cloud encountered an error.");
                ModelState.AddModelError("DbFail", ex.Message);
                return View(group);
            }

            if (group == null)
            {
                return NotFound();
            }
            return View(group);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {

            Group group = groupRepo.FindGroupByID(id).Group;
            var userdb = _identityRepo;
            {

                if (groupRepo.GetGroups(group.Community.ID, id).Count() == 1)
                {
                    if (userdb.GetUsersByGroupID(id).Count() == 0)
                    {
                        try
                        {
                            groupRepo.DeleteGroup(id);
                            userAHM.AddActivity("Delete group", ActivityObjectType.Group, group.Name);
                            return RedirectToAction("Index");
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, "Geni-View Cloud encountered an error.");
                            ModelState.AddModelError("DbFail", ex.Message);
                        }
                    }
                    else
                    {
                        string log = "Can't delete group, group has assigned ssers,  please delete ssers first or assign to a new group.";
                        _logger.Warn(log);
                        ModelState.AddModelError("DbFail", log);
                    }
                }
                else
                {
                    string log = "Can't delete group, group has children groups.";
                    _logger.Warn(log);
                    ModelState.AddModelError("DbFail", log);
                }
            }

            return View(group);
        }
        protected override void Dispose(bool disposing)
        {
            groupRepo.Dispose();
            communityRepo.Dispose();
            base.Dispose(disposing);
        }
    }
}
