# simple C# webring

supports multiple domains per person

add all users into the list.txt file next to the binary, separate the username and domains using a semicolon, and each domain using a comma

you can optionally include a link to an 88x31 badge by appending another semicolon, then the url

```
birb;https://miaow.ing/,https://birb.cc/,https://hummusbird.co.uk/
bobthebob;http://joke.enterprises/
alyxia;https://alyxia.dev/;https://alyxia.dev/static/img/88x31/self.png
```

ask each user to place this on their site. They are free to customise and style it

change the domain as appropriate

```html
<h2 style="text-align:center">
    <a href=https://webring.birb.cc/prev>&lt;=</a> <a href="https://webring.birb.cc/">webring</a> <a href=https://webring.birb.cc/next>=&gt;</a>
</h2>
```

there are four endpoints:

```
/     - root page

/next - uses the referrer header to find the next website in the ring

/prev - uses the referrer header to find the previous website in the ring

/rand - selects a random website from the ring

/members - returns a list of every member, their domains, and their 88x31 badge if defined
```

if the program cannot find the referrer in the webring, a random website will be selected

each user has a equal chance of being selected. the first domain in their list will always be chosen.
