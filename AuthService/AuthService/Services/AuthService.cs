using AuthService.Consumers;
using AuthService.DTO;
using AuthService.GenerateToken;
using AuthService.Messages;
using AuthService.Models;
using AuthService.Repositories.IRepositories;
using AuthService.Services.IServices;
using AuthService.Validators.IValidators;
using DemoApp1.Repositories.IRepositories;
using FluentValidation;
using FluentValidation.Results;
using MassTransit;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace AuthService.Services;

public class AuthService : IAuthService
    {
        private readonly TokenGenerator _tokenGenerator;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<Refreshtoken> _refreshTokenRepository;
        private readonly IGenericRepository<Role> _roleRepository; 
        private readonly IUserRepository _userRepository1;
        private readonly IValidatorService<User> _userValidator;
        private readonly IValidatorService<Refreshtoken> _refreshtokenValidator;
        private readonly IBus _iBus;

        public AuthService(TokenGenerator tokenGenerator, IGenericRepository<User> userRepository, IGenericRepository<Refreshtoken> refreshTokenRepository, IGenericRepository<Role> roleRepository, IUserRepository userRepository1, IValidatorService<User> userValidator, IValidatorService<Refreshtoken> refreshtokenValidator, IBus iBus)
        {
            _tokenGenerator = tokenGenerator;
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _roleRepository = roleRepository;
            _userRepository1 = userRepository1;
            _userValidator = userValidator;
            _refreshtokenValidator = refreshtokenValidator;
            _iBus = iBus;
        }


        public async Task<User> RegisterUserAsync(RegisterRequestDTO request)
        {
            // Check if email already exists
            var existingUser = (await _userRepository.GetAllAsync())
                .FirstOrDefault(u => u.Email == request.Email);

            if (existingUser != null)
            {
                throw new InvalidOperationException("Email already in use.");
            }
            
            // Fetch the role by name (must be a valid role)
            var role = (await _roleRepository.GetAllAsync())
                .FirstOrDefault(r => r.Name == request.Role);
            Console.WriteLine($"Role: {role}");

            if (role == null)
            {
                throw new InvalidOperationException("Invalid role.");
            }


            // Create the new user with the valid roles
            var newUser = new User
            {
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Username = request.Username,
                Role = role,  
                CreatedAt = DateTime.UtcNow
            };

            newUser.RoleId = newUser.Role.Id;
           //Console.WriteLine(newUser.RoleId);
            ValidationResult validationResult = await _userValidator.ValidateAsync(newUser);

            if (!validationResult.IsValid)
            {
                // If validation fails, create a collection of ValidationFailure objects
                var validationFailures = validationResult.Errors.Select(e => new ValidationFailure(e.PropertyName, e.ErrorMessage)).ToList();

                // Throw the ValidationException with the collection of ValidationFailure objects
                throw new ValidationException(validationFailures);
            }

            // Save the user to the database
            await _userRepository.AddAsync(newUser);

            if (newUser.Role.Name == "doctor")
            {
                await _iBus.Publish(new UserRegistered
                {
                    UserId = newUser.Id,
                    Email = newUser.Email,
                    Role = newUser.Role.Name,
                    RegisteredAt = DateTime.UtcNow,
                    Username = newUser.Username
                }, context =>
                {
                    context.SetRoutingKey("doctor");
                });
            }
            else if (newUser.Role.Name == "patient")
            {
                await _iBus.Publish(new UserRegistered
                {
                    UserId = newUser.Id,
                    Email = newUser.Email,
                    Role = newUser.Role.Name,
                    RegisteredAt = DateTime.UtcNow,
                    Username = newUser.Username
                }, context =>
                {
                    context.SetRoutingKey("patient");
                });
            }


            return newUser;
        }

        public async Task<(string accessToken, string refreshToken)> LoginUserAsync(LoginRequestDTO request)
        {
            //Console.WriteLine("Email: " + request.Email);  // Log email
            //Console.WriteLine("Password: " + request.Password);  // Log password
            
            // Fetch user with roles using the new method
            var user = await _userRepository1.GetUserWithRolesAsync(request.Email);
            if (user == null)
            {
                Console.WriteLine("No user found with email: " + request.Email);
            }
                
            // If user doesn't exist or password doesn't match
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }   
            
            // Generate tokens (access and refresh)
            var roleNames = user.Role.Name;
            Console.WriteLine("Role Names: " + roleNames);
            var accessToken = _tokenGenerator.GenerateToken(user.Id, user.Email, roleNames);
            var refreshToken = _tokenGenerator.GenerateRefreshToken();

            // Remove existing refresh tokens
            var existingRefreshTokens = (await _refreshTokenRepository.GetAllAsync())
                .Where(rt => rt.UserId == user.Id)
                .ToList();

            // If there are existing refresh tokens, delete them
            if (existingRefreshTokens.Any())  // Check if the list is not empty
            {
                foreach (var token in existingRefreshTokens)
                {
                    await _refreshTokenRepository.DeleteAsync(token.Id);  // Delete each refresh token
                }
            }
            else
            {
                Console.WriteLine("No existing refresh tokens found for the user.");
            }


            var refreshTokenEntity = new Refreshtoken
            {
                Token = refreshToken,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                UserId = user.Id
            };
            //Console.WriteLine("Token: " + refreshTokenEntity);
            
            ValidationResult validationResult3 = await _refreshtokenValidator.ValidateAsync(refreshTokenEntity);

            if (!validationResult3.IsValid)
            {
                // If validation fails, create a collection of ValidationFailure objects
                var validationFailures = validationResult3.Errors.Select(e => new ValidationFailure(e.PropertyName, e.ErrorMessage)).ToList();

                // Throw the ValidationException with the collection of ValidationFailure objects
                throw new ValidationException(validationFailures);
            }

            await _refreshTokenRepository.AddAsync(refreshTokenEntity);
            
            /*
            object details = null;
            if (user.Role.Name == "user")
            {
                details = await _userService.GetPatientByUserId(user.Id);
            }

            if (user.Role.Name == "doctor")
            {
                details = await _userService.GetDoctorByUserId(user.Id);
            }
            */

            // Return only the access token and refresh token
            
            // Publish the login event message with tokens included
            if (user.Role.Name == "doctor")
            {
                await _iBus.Publish(new UserLoggedIn
                {
                    UserId = user.Id,
                    Email = user.Email,
                    Role = user.Role.Name,
                    LoginTime = DateTime.UtcNow,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken
                }, context => { context.SetRoutingKey("doctor"); });
            }
            else if (user.Role.Name == "patient")
            {
                await _iBus.Publish(new UserLoggedIn
                {
                    UserId = user.Id,
                    Email = user.Email,
                    Role = user.Role.Name,
                    LoginTime = DateTime.UtcNow,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken
                }, context => { context.SetRoutingKey("patient"); });
            }


            /*var details = (object)null;
            if (user.Role.Name == "patient")
            {
                // Wait for the patient data to be processed and fetched
                details = PatientLoggedInConsumer.GetPatientDetails(user.Id); // Use int here for patientId

                if (details == null)
                {
                    throw new Exception("Patient details could not be retrieved.");
                }
            }*/


            return (accessToken, refreshToken);
        }
        
    }