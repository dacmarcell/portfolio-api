using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using portfolio_api.Data;
using portfolio_api.Dtos.Profile;
using portfolio_api.Models;

namespace portfolio_api.Controllers{
    [ApiController]
    [Route("api/profile")]
    public class ProfileController: ControllerBase{
        private readonly ILogger<ProfileController> _logger;
        private readonly ApplicationDBContext _dbContext;
        public ProfileController(ILogger<ProfileController> logger, ApplicationDBContext dbContext){
            _logger = logger;
            _dbContext = dbContext;
        }

        [HttpGet(Name = "GetProfile")]
        public ActionResult<Profile> GetProfile(){
            var profiles = _dbContext.Profiles;
            return Ok(profiles);
        }

        [HttpGet("{id}", Name = "GetProfileById")]
        public async Task<ActionResult<Profile>> GetProfileById(int id){
            var profile = await _dbContext.Profiles.FindAsync(id);
            if (profile is null){
                return NotFound("Profile not found");
            }

            return Ok(profile);
        }

        [HttpPost(Name = "CreateProfile")]
        public async Task<ActionResult<Profile>> CreateProfile(Profile profile){
            _dbContext.Profiles.Add(profile);
            await _dbContext.SaveChangesAsync();
            return CreatedAtRoute("CreateProfile", new { id = profile.ID }, profile);
        }

        [HttpPost("{id}/social-media", Name = "ConnectOrDisconnectSocialMedia")]
        public async Task<ActionResult<Profile>> ConnectOrDisconnectSocialMedia(int id, [FromBody] ConnectOrDisconnectSocialMediaDto body){
            var profile = await _dbContext.Profiles
                .Include(profile => profile.SocialMedia)
                .FirstOrDefaultAsync(profile => profile.ID == id);
                
            if (profile is null) {
                return NotFound("Profile not found");
            }

            var socialMedia = await _dbContext.SocialMedias.FindAsync(body.SocialMediaID);
            if (socialMedia is null) {
                return NotFound("Social Media not found");
            }

            if (body.Kind == SocialMediaKind.Connect) {
                profile.SocialMedia.Add(socialMedia);
            } else if (body.Kind == SocialMediaKind.Disconnect) {
                profile.SocialMedia.Remove(socialMedia);
            } else {
                return BadRequest("Invalid Kind");
            }
            
            _dbContext.Profiles.Update(profile);

            await _dbContext.SaveChangesAsync();
            return Ok(profile);
        }

        [HttpPatch("{id}", Name = "UpdateProfile")]
        public async Task<ActionResult<Profile>> UpdateProfile(int id, [FromBody] Profile body){
            var foundProfile = await _dbContext.Profiles.FindAsync(id);
            if(foundProfile is null){
                return NotFound("Profile not found");
            }

            if(!string.IsNullOrWhiteSpace(body.Name)){
                foundProfile.Name = body.Name;
            } else if (!string.IsNullOrWhiteSpace(body.AboutMe)){
                foundProfile.AboutMe = body.AboutMe;
            }

            _dbContext.Profiles.Update(foundProfile);
            await _dbContext.SaveChangesAsync();

            return Ok(foundProfile);
        }

        [HttpDelete("{id}", Name = "DeleteProfile")]
        public async Task<ActionResult<Profile>> DeleteProfile(int id){
            var profile = await _dbContext.Profiles.FindAsync(id);
            if(profile is null){
                return NotFound("Profile not found");
            }

            _dbContext.Profiles.Remove(profile);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}