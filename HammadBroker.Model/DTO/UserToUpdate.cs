using System;
using System.ComponentModel.DataAnnotations;

namespace HammadBroker.Model.DTO;

public class UserToUpdate : UserToUpdateCore
{
	[Required]
	[RegularExpression("^[a-zA-Z0-9_@\\-\\.\\+]+$")]
	[Display(Name = "اسم المستخدم")]
	public string UserName { get; set; }


	[Display(Name = "البريد الالكتروني مؤكد")]
	public bool EmailConfirmed { get; set; }

	[Display(Name = "رقم التليفون مؤكد")]
	public bool PhoneNumberConfirmed { get; set; }

	[Display(Name = "الحظر مفعل")]
	public bool LockoutEnabled { get; set; }

	[Display(Name = "المحاولات الفاشلة")]
	public int AccessFailedCount { get; set; }

	[Display(Name = "انتهاء الحظر")]
	public DateTimeOffset? LockoutEnd { get; set; }
}