Sometimes objects can't be easily compared thanks to Metadata. Consider this object for instance

    public class Person {
    	public Guid Guid {get;set;}
    	public DateTime CreatedOn {get;set;}
    	public string FirstName {get;set;}
    	public string LastName {get;set;}
    }

Now, if I were working with this entity and an API, I might do something like this

	public void TestPersonCreation() {
		var p = new Person () {
			FirstName = "John",
			LastName = "Doe"
		}
    
		//API returns person object, fields should be identical, but some fields are autopopulated now
		var resultantPerson = API.CreatePerson(p);
    
		Assert.AreEqual(p, resultantPerson);
	}

This would lead to a failed test, why? Because the API updated some fields with values that I don't care about. While testing for business purposes, I don't actually care that the guid (used in database indexing) is updated, or that the CreatedOn field is now set. In order to verify the object saved correctly, I need to add an Assert.AreEqual for every field I'm interested in. For the simple class above, that's fine, but if I were doing something more complex (address? phone numbers?) it would get very repetitive. 

The key issue here is that we have a class, some of the information in the class is relevant in one sense, but not relevant in another. If I'm testing the API saves my fields properly, I many not be interested in testing if it also auto updates the guid.

####Enter Comparish

Now lets take a look at how we can use comparish to simply this process. First we need to update our Person class with some attributes.

    public class Person {
    	[DataMeaning("Metadata")]
    	public Guid Guid {get;set;}

    	[DataMeaning("Metadata")]
    	public DateTime CreatedOn {get;set;}

    	[DataMeaning("BusinessData")]
    	public string FirstName {get;set;}

    	[DataMeaning("BusinessData")]
    	public string LastName {get;set;}
    }

Now we can update our test too

	public void TestPersonCreation() {
		var p = new Person () {
			FirstName = "John",
			LastName = "Doe"
		}
    
		//API returns person object, fields should be identical, but some fields are autopopulated now
		var resultantPerson = API.CreatePerson(p);
    
		Assert.IsTrue(new Comparishon(p, resultantPerson, "BusinessData"));
	}

And bam, the test only tests to ensure the fields we're interested in match.