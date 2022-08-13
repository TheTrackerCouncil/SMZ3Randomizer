# Tracker Config Profiles

You can create new profiles for tracker responses which can be enabled or disabled in the tracker options. While changes to the build in profiles will be overridden with updates, these new profiles will be left alone.

All selected profiles will be merged when opening tracker.

## Creating a new profile

To create a new profile, navigate to the directory by navigating to %localappdata%\SMZ3CasRandomizer\Configs or by clicking on the profiles folder in the options window and create a new folder with the profile name you desire.

You can then either copy the yml files from the Templates folder or create an empty yml file matching the following names:

- **bosses.yml** - Additional boss information
- **dungeons.yml** - Additional dungeons information
- **items.yml** - Additional items information
- **locations.yml** - Additional locations information
- **regions.yml** - Additional region information
- **requests.yml** - Adds addition prompt and responses for tracker
- **responses.yml** - Responses for specific actions performed by the user
- **rewards.yml** - Additional reward information
- **rooms.yml** - Additional room information

You can open the template yaml files for additional details on what can be modified in each file.

## Editing profile configs

Profile configs are written in a language called YAML. You can edit these in any editor (though something like Visual Studio Code is suggested), and you can find YAML validators online which may help find issues with Tracker loading your YAML files. More detailed specifications can be found elsewhere, but some very basic details:

- Values are specified via the format name: value
- Series of items are prefixed with -
- Comments are ignored lines and start with #
- Whitespacing is important

## Config merging

All selected profiles are merged, combining all responses together into a single list. Each config type is merged based on an identifier, which is identified in the comments at the top of each config file.

When loading a config file, it will find if any previous configs already had an entry with that identifier. If it does, it will combine the responses for the two configs. If it doesn't, it'll add it as a new entry. Using this you can add your own items, bosses, etc. which can be tracked.

## Possibilities

Most of the settings are a list of possibilities, which is a series of messages that tracker can say and the likelyhood in which they will be said. Possibilities will show up like this:

```
Field:
- Text: One message that can be stated
- Text: Another message that can be stated
- Text: This message is less likely to be said
  Weight: 0.1
- Text: This message is also less likely to be said
  Weight: 0.1
```

The text line is required and is what will be said by tracker. The weight adjusts the likelyhood of the message being said. This field is optional, and if no weight is specified it defaults to a value of 1. The higher the value the more likely it is to be said. The closer a value is to 0, the less likely it is to be said.

## Questions

### Can I use some responses from one config, but not all?

Unfortunately you can't pick and choose what comes from a config. You can either copy the profile and remove what you don't want. Or, if you have your own profile, you can increase the weight of your responses to make them way more likely than the original responses.
