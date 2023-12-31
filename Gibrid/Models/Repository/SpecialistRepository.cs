﻿using Gibrid.Models.Interfaces;
using Gibrid.VewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gibrid.Models.Repository
{
    public class SpecialistRepository:IAllSpecialist
    {
        private readonly AppDBContent _content;
        private readonly ISpecialistCategory carRep;
        private readonly IReview reviewRep;
        private readonly UserManager<User> _userManager;
        public SpecialistRepository(AppDBContent content, ISpecialistCategory carRep, UserManager<User> userManager, IReview reviewRep)
        {
            _content = content;
            this.carRep = carRep;
            _userManager = userManager;
            this.reviewRep = reviewRep;
        }
        public void createSpecialist(SpecialistViewModel specialist)
        {
            byte[] imageData = null;
            var category = carRep.AllCategories.SingleOrDefault(x => x.Name == specialist.Category);
            category.isEmpty = true;
           
            if (specialist.Avatar != null)
            {               
                // считываем переданный файл в массив байтов
                using (var binaryReader = new BinaryReader(specialist.Avatar.OpenReadStream()))
                {
                    imageData = binaryReader.ReadBytes((int)specialist.Avatar.Length);
                }
              
            }
            User user = _userManager.FindByNameAsync(specialist.UserName).Result;
            var specialistDetails = new Specialist()
            {
                Name= specialist.Name,
                SurName= specialist.SurName,
                FullName= specialist.FullName,
                Phone= specialist.Phone,
                isHasTime=false,
                userId= user.Id,
                Experience= specialist.Experience,
                Rating =0,
                Avatar=imageData,
                CategoryId=category.Id,
                CategoryName = specialist.Category
            };
                _content.Specialist.Add(specialistDetails);//добавление специалиста в сооответствующую таблицу в БД
       
            _content.SaveChanges();//сохранение изменений в БД
        }
        public void ChangeRating(Specialist specialist, int rating)
        {//пересчет рейтинга мастера
            var countReviews = reviewRep.allReviewsDetails.Where(x => x.SpecialistId == specialist.Id).Count();
            countReviews += 1;
            var sumRatings = reviewRep.allReviewsDetails.Where(x => x.SpecialistId == specialist.Id).Sum(x => x.Rating);
            sumRatings += rating;
            specialist.Rating = (int)Math.Round((double)(sumRatings/countReviews));//установка нового рейтинга
            _content.SaveChanges();//сохранение изменений в БД
        }
        public void Update(Specialist specialist)
        {
            _content.Specialist.Update(specialist);//обновление специалиста в БД

            _content.SaveChanges();//сохранение изменений в БД
        }
        public void Delete(Specialist specialist)
        {
           
           specialist.isDelete = true;
            _content.Update(specialist);//обновление специалиста в БД
            _content.SaveChanges();//сохранение изменений в БД
        }
        public IEnumerable<CategorySpecialist> Categories => _content.CategorySpecialist;//получение всех категорий из БД
        public IEnumerable<Specialist> Specialists => _content.Specialist;//получение всех мастеров из БД

        public IEnumerable<Time> SpecialistTimes(int id) => _content.Time.Where(x=>x.SpecialistDetailsId==id);//получение списка времен мастера по его айди из БД
        public Specialist getObjectSpecialist(int spId) => _content.Specialist.SingleOrDefault(sp => sp.Id == spId);//получение мастера по его айди из БД
    }
}
