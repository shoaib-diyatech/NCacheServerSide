namespace NCacheClient;
using System;
using System.Reflection.Metadata.Ecma335;
using Newtonsoft.Json;

[Serializable]
public class Subscriber
{
    public string Msisdn { get; set; }
    public string Name { get; set; }

    //public Gender Gender { get; set; }
    public string Email { get; set; }

    public bool IsActive { get; set; }

    [NonSerialized]
    private DateOnly _dateOfBirth;

    [JsonIgnore]
    public DateOnly DateOfBirth
    {
        get => _dateOfBirth;
        set => _dateOfBirth = value;
    }

    public string DateOfBirthString
    {
        get => _dateOfBirth.ToString("dd-MM-yyyy");
        set => _dateOfBirth = DateOnly.Parse(value);
    }
    
    public long Id { get; set; }

    /// <summary>
    /// Serialize the Subscriber object to JSON
    /// </summary>
    /// <param name="subscriber"></param>
    /// <returns></returns>
    public static string Serialize(Subscriber subscriber)
    {
        return JsonConvert.SerializeObject(subscriber);
    }

    /// <summary>
    /// Deserialize JSON to a Subscriber object
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    public static Subscriber Deserialize(string json)
    {
        return JsonConvert.DeserializeObject<Subscriber>(json);
    }

    public static Subscriber Parse(string json)
    {
        return Deserialize(json);
    }

    public override string ToString()
    {
        return $"Subscriber: {Name} ({Msisdn})";
    }

    public static Subscriber GetRandomSubscriber()
    {
        Random random = new Random();
        int randomDay = random.Next(1, 28);
        int randomMonth = random.Next(1, 12);
        int randomYear = random.Next(1970, 2000);
        int id = random.Next(1000, 9999);
        string[] randomNames = { "John Doe", "Jane Doe", "Alice", "Bob", "Charlie", "David", "Eve", "Frank", "Grace", "Heidi",
            "Ivan", "Jack", "Kathy", "Liam", "Mia", "Nina", "Oliver", "Pam", "Quinn", "Riley", "Sara", "Tom", "Uma", "Vera", "Will", "Xander", "Yara", "Zara", "Zoe" , "Zack"
            , "Fever", "Frost", "Gale", "Garnet", "Ginger", "Gizmo", "Goblin", "Goldie", "Goober", "Goose", "Gopher", "Grizzly", "Gulliver", "Guppy", "Gus", "Gypsy", "Haley", "Hank", "Harley", "Harper", "Hawk", "Hazel", "Heath", "Hercules", "Hershey", "Holly", "Honey", "Honor", "Hope", "Hudson", "Hunter", "Iggy", "Indigo", "Inky", "Iris", "Isis", "Ivory", "Ivy", "Jade", "Jagger", "Jaguar", "Jazz", "Jellybean", "Jewel", "Jinx", "Joey", "Journey", "Joy", "Jude", "Jules", "Juliet", "June", "Juno", "Jupiter", "Kai", "Karma", "Kash", "Keanu", "Keats", "Keiko", "Kiki", "King", "Kip", "Kismet", "Klaus", "Koda", "Kodiak", "Kona", "Kosmo", "Kovu", "Kuma", "Kyoto", "Lark", "Layla", "Legend", "Lemon", "Leo", "Levi", "Lilac", "Lily", "Lincoln", "Lionel", "Lola", "Lucky", "Lulu", "Luna", "Lyric", "Mabel", "Mack", "Maddox", "Maggie", "Mango", "Maple", "Marley", "Mars", "Maverick", "Max", "Maya", "Meadow", "Mercury", "Mia", "Midnight", "Mika", "Miles", "Milo", "Mimi", "Minnie", "Misty", "Mocha", "Moe", "Molly", "Monkey", "Monty", "Moon", "Moose", "Morgan", "Mowgli", "Muffin", "Mulan", "Munchkin", "Murray", "Mya",
            "Nala", "Nash", "Nell", "Nemo", "Neptune", "Nico", "Nikita", "Niko", "Nina", "Ninja", "Noah", "Nola", "Noodle", "Nugget", "Nutmeg", "Oakley", "Oasis", "Odie", "Olive", "Olivia", "Ollie", "Onyx", "Opal", "Oreo", "Orion", "Otis", "Otto", "Ozzy", "Pablo", "Paisley", "Panda", "Pandora", "Pansy", "Parker", "Pasha", "Peanut", "Pearl", "Pebbles", "Penny", "Pepper", "Petunia", "Phoebe", "Pickle", "Piper", "Pippin", "Pistol", "Pixie", "Pluto", "Pogo", "Polly", "Poppy", "Porter", "Posey", "Preston", "Prince", "Princess", "Priscilla", "Puck", "Puddles", "Pumpkin", "Puppy", "Pyro", "Quincy", "Quinn", "Radar", "Ralph", "Ranger", "Rascal", "Raven", "Rebel", "Reese", "Rex", "Rhubarb", "Rico", "Riley", "Ringo", "Rio", "Ripley", "River", "Rocco", "Rocky", "Rogue", "Romeo", "Roo", "Roscoe", "Rose", "Rosie", "Rover", "Ruby", "Rudy", "Rufus", "Rusty", "Sadie", "Sage", "Sahara", "Salem", "Sam", "Samantha", "Sammy", "Samson", "Sandy", "Sapphire", "Sarge", "Sasha", "Sassy", "Scooby", "Scout", "Scrappy", "Shadow", "Shasta", "Shelby", "Sherlock", "Shiloh", "Simba", "Sissy", "Skippy", "Sky", "Smokey", "Snickers",
            "Snoopy", "Snowball", "Socks", "Sophie", "Sparrow", "Spencer", "Spike", "Spirit", "Spot", "Sprout", "Squirt", "Stella", "Stitch", "Storm", "Sugar", "Suki", "Sully", "Sunny", "Sunshine", "Susie", "Sylvester", "Taco", "Taffy", "Tasha", "Taz", "Teddy", "Tesla", "Theo", "Thor", "Tiger", "Tilly", "Timber", "Toby", "Tucker", "Tulip",
            "Turtle", "Kuala", "Kangaroo", "Koala", "Kookaburra", "Koel", "Justin", "George", "Peter", "Jane"};
        string name = randomNames[random.Next(0, randomNames.Length)];
        return new Subscriber
        {
            Msisdn = "+1" + random.Next(100000000, 999999999).ToString(),
            Id = id,
            DateOfBirth = new DateOnly(randomYear, randomMonth, randomDay),
            Name = name,
            IsActive = id % 2 == 0,
            Email = name + id + "@example.com",
        };
    }

    public int GetAge()
    {

        return DateTime.Now.Year - DateOfBirth.Year;
        // return 10;

    }
}

[Serializable]
public enum ServiceType
{
    Postpaid,
    Prepaid
}

public enum Gender
{
    Male,
    Female
}