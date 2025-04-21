using System;
using System.ComponentModel.DataAnnotations;

public class ValidBirthDateAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value is DateTime birthDate)
        {
            var minDate = new DateTime(1900, 1, 1);
            var maxDate = DateTime.Today.AddYears(-18); // En az 18 yaşında olmalı

            return birthDate >= minDate && birthDate <= maxDate;
        }

        return false;
    }

    public override string FormatErrorMessage(string name)
    {
        return "18 yaşından büyük olmalısınız.";
    }
}