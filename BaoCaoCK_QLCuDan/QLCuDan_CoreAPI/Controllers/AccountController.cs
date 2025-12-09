using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QLCuDan_CoreAPI.DTO;
using QLCuDan_CoreAPI.Repository;

namespace QLCuDan_CoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        public IAccountRepository _accountRepository;
        public AccountController(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }
        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] SignUpModel signUpModel)
        {
            try
            {
                var result = await _accountRepository.SignUpAsync(signUpModel);
                if (!result.Succeeded)
                {
                    return BadRequest(result.Errors);
                }
                return Ok("User registered successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpPost("SignIn")]
        public async Task<IActionResult> SignIn([FromBody] SignInModel signInModel)
        {
            try
            {
                var result = await _accountRepository.SignInAsync(signInModel);
                if (result == "Invalid Authentication")
                {
                    return Unauthorized();
                }
                return Ok(new { Token = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}

